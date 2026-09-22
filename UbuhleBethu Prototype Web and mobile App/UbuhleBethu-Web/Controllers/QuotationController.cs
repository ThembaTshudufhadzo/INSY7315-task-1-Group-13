using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UbuhlebethuConnectPro.Web.Data;
using UbuhlebethuConnectPro.Web.Models;

namespace UbuhlebethuConnectPro.Web.Controllers
{
    public class QuotationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public QuotationController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(QuoteRequest request)
        {
            if (ModelState.IsValid)
            {
                request.RequestDate = DateTime.Now;
                request.Reference = $"UBU-{Random.Shared.Next(1000, 9999)}";
                request.Status = "Submitted";
                request.CreatedBy = User?.Identity?.Name ?? request.ClientName;

                _db.QuoteRequests.Add(request);
                await _db.SaveChangesAsync();

                // Create a simple notification for office/admin users (prototype)
                var notif = new Notification
                {
                    UserId = "office",
                    Message = $"New quotation submitted: {request.Reference} by {request.ClientName}",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    ReferenceId = request.Reference
                };
                _db.Notifications.Add(notif);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Quotation request submitted successfully! Ref: {request.Reference}";
                return RedirectToAction("Confirmation");
            }
            return View(request);
        }

        [Authorize]
        public IActionResult Confirmation()
        {
            ViewBag.Message = TempData["SuccessMessage"];
            return View();
        }

        // Client/authorized user can view a submitted quotation
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var quote = await _db.QuoteRequests.FindAsync(id);
            if (quote == null) return NotFound();

            return View(quote);
        }

        // Stub payment: mark quotation as paid and create an invoice record
        [Authorize(Roles = "Client,Admin")]
        [HttpPost]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var quote = await _db.QuoteRequests.FindAsync(id);
            if (quote == null) return NotFound();

            if (quote.IsPaid)
            {
                TempData["SuccessMessage"] = "Already marked as paid.";
                return RedirectToAction("Details", new { id });
            }

            // Create invoice (mock calculation: use EstimatedBudget)
            var invoice = new Invoice
            {
                QuoteRequestId = quote.Id,
                Amount = quote.EstimatedBudget,
                CreatedAt = DateTime.UtcNow,
                IsPaid = true
            };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            quote.IsPaid = true;
            quote.InvoiceId = invoice.Id;
            quote.Status = "Paid";
            await _db.SaveChangesAsync();

            // Create notification for client
            var notif = new Notification
            {
                UserId = quote.CreatedBy,
                Message = $"Payment received for {quote.Reference}. Invoice #{invoice.Id} generated.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                ReferenceId = quote.Reference
            };
            _db.Notifications.Add(notif);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment recorded and invoice generated (prototype).";
            return RedirectToAction("Details", new { id });
        }

        // --- Office/QS Actions ---
        [Authorize(Roles = "Office,Admin")]
        public async Task<IActionResult> Review(int id)
        {
            var quote = await _db.QuoteRequests.FindAsync(id);
            if (quote == null) return NotFound();
            return View(quote);
        }

        [Authorize(Roles = "Office,Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var quote = await _db.QuoteRequests.FindAsync(id);
            if (quote == null) return NotFound();
            return View(quote);
        }
    }
}
