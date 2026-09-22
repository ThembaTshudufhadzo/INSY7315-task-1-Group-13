using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UbuhlebethuConnectPro.Web.Data;

namespace UbuhlebethuConnectPro.Web.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _db;

        public InvoiceController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Preview(int id)
        {
            var invoice = _db.Invoices.FirstOrDefault(i => i.Id == id);
            if (invoice == null) return NotFound();
            var quote = _db.QuoteRequests.FirstOrDefault(q => q.Id == invoice.QuoteRequestId);
            ViewBag.Quote = quote;
            return View(invoice);
        }

        public IActionResult Download(int id)
        {
            var invoice = _db.Invoices.FirstOrDefault(i => i.Id == id);
            if (invoice == null) return NotFound();
            var quote = _db.QuoteRequests.FirstOrDefault(q => q.Id == invoice.QuoteRequestId);

            var html = RenderInvoiceHtml(invoice, quote);
            var bytes = System.Text.Encoding.UTF8.GetBytes(html);
            var fileName = $"invoice-{invoice.Id}.html";
            return File(bytes, "text/html", fileName);
        }

        private string RenderInvoiceHtml(UbuhlebethuConnectPro.Web.Models.Invoice invoice, UbuhlebethuConnectPro.Web.Models.QuoteRequest? quote)
        {
            var client = quote?.ClientName ?? "Unknown";
            var reference = quote?.Reference ?? "N/A";
            var amount = invoice.Amount.ToString("C");
            var created = invoice.CreatedAt.ToLocalTime().ToString("g");

            return $@"<!doctype html>
<html>
<head><meta charset='utf-8'><title>Invoice {invoice.Id}</title></head>
<body>
<h2>Ubuhlebethu ConnectPro - Invoice #{invoice.Id}</h2>
<p><strong>Client:</strong> {client}</p>
<p><strong>Reference:</strong> {reference}</p>
<p><strong>Amount:</strong> {amount}</p>
<p><strong>Date:</strong> {created}</p>
<hr />
<p>This is a prototype invoice (HTML). For production, deliver PDF or signed invoice.</p>
</body></html>";
        }
    }
}
