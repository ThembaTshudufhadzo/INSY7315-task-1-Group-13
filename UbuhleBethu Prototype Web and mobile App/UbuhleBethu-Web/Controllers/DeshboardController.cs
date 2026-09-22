using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UbuhlebethuConnectPro.Web.Models;

namespace UbuhlebethuConnectPro.Web.Controllers
{
    public class DashboardController : Controller
    {
        private static readonly List<Job> MockJobs = new List<Job>
        {
            new Job { Id = 1, JobNumber = "J1020", Description = "Sandton Highrise Extension", BuildingType = "Commercial Build", ClientName = "Steve MacDonald", Location = "Gauteng", Progress = 45, Variance = 12, EstimatedCost = 4500000m, InvoicedAmount = 2025000m, SiteManager = "Mhlengi Ndlovu", NhbrcCompliant = true },
            new Job { Id = 2, JobNumber = "J1021", Description = "Waterfall Estate Phase 3", BuildingType = "Residential Complex", ClientName = "Mogale City Municipality", Location = "Midrand", Progress = 88, Variance = -2, EstimatedCost = 12500000m, InvoicedAmount = 11000000m, SiteManager = "Musa Khumalo", NhbrcCompliant = true },
            new Job { Id = 3, JobNumber = "J1022", Description = "Cape Town Waterfront Repairs", BuildingType = "Civil Engineering", ClientName = "Dept of Infrastructure", Location = "Western Cape", Progress = 15, Variance = 22, EstimatedCost = 850000m, InvoicedAmount = 127500m, SiteManager = "Chuma Sithole", NhbrcCompliant = true },
            new Job { Id = 4, JobNumber = "J1023", Description = "Soweto Community Clinic", BuildingType = "Public Health Facility", ClientName = "Health Dept", Location = "Soweto", Progress = 100, Variance = 0, EstimatedCost = 3200000m, InvoicedAmount = 3200000m, SiteManager = "Sipho Mbele", NhbrcCompliant = true },
            new Job { Id = 5, JobNumber = "J1024", Description = "Durban Port Logistics Hub", BuildingType = "Industrial", ClientName = "Transnet", Location = "KwaZulu-Natal", Progress = 5, Variance = 5, EstimatedCost = 28000000m, InvoicedAmount = 1400000m, SiteManager = "Lerato Nkosi", NhbrcCompliant = false }
        };

        // 1. Primary Office & QS Dashboard
        [Authorize(Roles = "Office,Admin")]
        public IActionResult Index()
        {
            return View(MockJobs);
        }

        // 2. Client Portal Dashboard
        [Authorize(Roles = "Client,Admin")]
        public IActionResult ClientPortal(DateTime? from = null, DateTime? to = null, string status = null)
        {
            // Filter jobs relevant to the active logged-in client
            var clientJobs = MockJobs.Where(j => j.ClientName == "Steve MacDonald").ToList();

            // If user is authenticated, show their quotes as well and support simple filters
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                try
                {
                    using var scope = HttpContext.RequestServices.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<UbuhlebethuConnectPro.Web.Data.ApplicationDbContext>();
                    var username = User.Identity.Name ?? string.Empty;
                    var query = db.QuoteRequests.Where(q => q.CreatedBy == username).AsQueryable();
                    if (from.HasValue)
                        query = query.Where(q => q.RequestDate >= from.Value);
                    if (to.HasValue)
                        query = query.Where(q => q.RequestDate <= to.Value);
                    if (!string.IsNullOrEmpty(status))
                        query = query.Where(q => q.Status == status);

                    var clientQuotes = query.OrderByDescending(q => q.RequestDate).ToList();
                    ViewBag.ClientQuotes = clientQuotes;
                }
                catch
                {
                    ViewBag.ClientQuotes = new List<UbuhlebethuConnectPro.Web.Models.QuoteRequest>();
                }
            }

            return View(clientJobs);
        }

        // 3. Executive / Management Dashboard
        [Authorize(Roles = "Executive,Management,Admin")]
        public IActionResult Executive()
        {
            ViewBag.TotalProjects = MockJobs.Count;
            ViewBag.TotalValue = MockJobs.Sum(j => j.EstimatedCost);
            ViewBag.TotalInvoiced = MockJobs.Sum(j => j.InvoicedAmount);
            ViewBag.AvgProgress = (int)MockJobs.Average(j => j.Progress);
            return View(MockJobs);
        }

        // 4. Details for a specific job
        [Authorize]
        public IActionResult Details(int id)
        {
            var job = MockJobs.FirstOrDefault(j => j.Id == id) ?? MockJobs.First();
            return View(job);
        }

        // Additional role-focused dashboards (placeholders to align with UX Journey Map)
        // Office / Quantity Surveyor - shows pending quotes, invoice generation, drafts
        [Authorize(Roles = "Office,Admin")]
        public IActionResult Office()
        {
            return View();
        }

        [Authorize(Roles = "Office,Admin")]
        public IActionResult PendingQuotes()
        {
            // In a full implementation this would query pending quote requests
            return View();
        }



        // Field Worker removed from this project per UX scope (not implemented)

        // System Admin - user management, audit logs
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult UserMgmt()
        {
            return View();
        }

        // Management / Financial dashboard
        [Authorize(Roles = "Management,Admin")]
        public IActionResult Management()
        {
            ViewBag.TotalProjects = MockJobs.Count;
            ViewBag.TotalValue = MockJobs.Sum(j => j.EstimatedCost);
            return View(MockJobs);
        }

        // --- New Analytics & Reporting Views ---
        [Authorize]
        public IActionResult LiveStats()
        {
            return View(MockJobs);
        }

        [Authorize]
        public IActionResult PortfolioView()
        {
            return View(MockJobs);
        }

        [Authorize]
        public IActionResult Reports()
        {
            return View(MockJobs);
        }

        [Authorize]
        public IActionResult RegionalMap()
        {
            return View(MockJobs);
        }

        [Authorize]
        public IActionResult Export()
        {
            return View(MockJobs);
        }

        [Authorize]
        public IActionResult AIReport()
        {
            return View(MockJobs);
        }
    }
}