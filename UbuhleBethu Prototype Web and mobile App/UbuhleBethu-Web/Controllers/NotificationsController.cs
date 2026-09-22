using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UbuhlebethuConnectPro.Web.Data;

namespace UbuhlebethuConnectPro.Web.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public NotificationsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Returns a partial listing of recent notifications for the current user
        public IActionResult List()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var notifs = _db.Notifications.Where(n => n.UserId == username).OrderByDescending(n => n.CreatedAt).Take(10).ToList();
            return PartialView("_NotificationsList", notifs);
        }

        // Full notifications center
        public IActionResult Index()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var notifs = _db.Notifications.Where(n => n.UserId == username).OrderByDescending(n => n.CreatedAt).ToList();
            return View(notifs);
        }

        [HttpPost]
        public IActionResult MarkRead(int id)
        {
            var notif = _db.Notifications.Find(id);
            if (notif == null) return NotFound();
            notif.IsRead = true;
            _db.SaveChanges();
            return Ok();
        }
    }
}
