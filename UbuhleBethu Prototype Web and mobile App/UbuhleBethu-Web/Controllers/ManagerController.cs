using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UbuhlebethuConnectPro.Web.Data;

namespace UbuhlebethuConnectPro.Web.Controllers
{
    [Authorize(Roles = "Management,Admin")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ManagerController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var totalProjects = _db.Projects.Count();
            var totalValue = _db.Projects.Sum(p => (decimal?)p.EstimatedCost) ?? 0m;
            var totalInvoiced = _db.Invoices.Sum(i => (decimal?)i.Amount) ?? 0m;
            var totalPaid = _db.Invoices.Where(i => i.IsPaid).Sum(i => (decimal?)i.Amount) ?? 0m;

            ViewBag.TotalProjects = totalProjects;
            ViewBag.TotalValue = totalValue;
            ViewBag.TotalInvoiced = totalInvoiced;
            ViewBag.TotalPaid = totalPaid;

            var invoices = _db.Invoices.OrderByDescending(i => i.CreatedAt).Take(20).ToList();
            return View(invoices);
        }
    }
}
