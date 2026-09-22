using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UbuhlebethuConnectPro.Web.Data;
using UbuhlebethuConnectPro.Web.Models;

namespace UbuhlebethuConnectPro.Web.Controllers
{
    [Authorize(Roles = "Office,Admin")]
    public class OfficeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OfficeController(ApplicationDbContext db)
        {
            _db = db;
        }

        // List pending quotes
        public IActionResult PendingQuotes()
        {
            var pending = _db.QuoteRequests.Where(q => q.Status == "Submitted" || q.Status == "Draft").OrderByDescending(q => q.RequestDate).ToList();
            return View(pending);
        }

        // Preview a quote and perform actions
        public IActionResult PreviewQuote(int id)
        {
            var quote = _db.QuoteRequests.FirstOrDefault(q => q.Id == id);
            if (quote == null) return NotFound();
            return View(quote);
        }

        [HttpPost]
        public IActionResult GenerateDraftEstimate(int id)
        {
            var quote = _db.QuoteRequests.FirstOrDefault(q => q.Id == id);
            if (quote == null) return NotFound();

            // Mock AI draft estimate: base + length factor + type multiplier
            decimal baseCost = 50000m;
            var typeMultiplier = quote.ProjectType switch
            {
                var s when s.Contains("RDP", System.StringComparison.OrdinalIgnoreCase) => 1.0m,
                var s when s.Contains("Clinic", System.StringComparison.OrdinalIgnoreCase) => 2.0m,
                var s when s.Contains("School", System.StringComparison.OrdinalIgnoreCase) => 1.8m,
                _ => 1.5m
            };
            var descFactor = Math.Max(1, quote.ProjectDescription?.Length ?? 1) / 100.0m;
            var estimate = baseCost * typeMultiplier * descFactor;
            // ensure a sensible minimum
            if (estimate < 20000) estimate = 20000;

            quote.EstimatedBudget = decimal.Round(estimate, 2);
            quote.Status = "Reviewed";
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Draft estimate generated: {quote.EstimatedBudget:C}.";
            return RedirectToAction("PreviewQuote", new { id });
        }

        [HttpPost]
        public IActionResult SendInvoice(int id)
        {
            var quote = _db.QuoteRequests.FirstOrDefault(q => q.Id == id);
            if (quote == null) return NotFound();

            var invoice = new Invoice
            {
                QuoteRequestId = quote.Id,
                Amount = quote.EstimatedBudget,
                CreatedAt = DateTime.UtcNow,
                IsPaid = false
            };
            _db.Invoices.Add(invoice);
            _db.SaveChanges();

            quote.InvoiceId = invoice.Id;
            quote.Status = "Invoiced";
            _db.SaveChanges();

            // Create notification to client
            var notif = new Notification
            {
                UserId = quote.CreatedBy,
                Message = $"Invoice #{invoice.Id} sent for {quote.Reference}.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                ReferenceId = quote.Reference
            };
            _db.Notifications.Add(notif);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Invoice generated and sent to client (prototype).";
            return RedirectToAction("PreviewQuote", new { id });
        }

        [HttpPost]
        public IActionResult ApproveQuote(int id)
        {
            var quote = _db.QuoteRequests.FirstOrDefault(q => q.Id == id);
            if (quote == null) return NotFound();
            quote.Status = "Approved";
            // create a project record from the quote
            var project = new UbuhlebethuConnectPro.Web.Models.Project
            {
                Title = $"Project - {quote.Reference}",
                ClientName = quote.ClientName,
                QuoteReference = quote.Reference,
                StartDate = DateTime.UtcNow,
                EstimatedCost = quote.EstimatedBudget,
                Status = "Active"
            };
            _db.Projects.Add(project);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Quote approved and project created (prototype).";
            return RedirectToAction("PendingQuotes");
        }
    }
}
