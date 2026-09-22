using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using UbuhlebethuConnectPro.Web.Data;
using UbuhlebethuConnectPro.Web.Models;

namespace UbuhlebethuConnectPro.Web.Controllers
{
    [Authorize(Roles = "Admin")]

    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }



        // Enhanced index with search and role filter
        public async Task<IActionResult> Index(string? search, string? role)
        {
            var usersQuery = _userManager.Users.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));
            }

            var users = usersQuery.ToList();
            var model = new List<AdminUserViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                if (!string.IsNullOrEmpty(role) && !roles.Contains(role)) continue;
                model.Add(new AdminUserViewModel { Id = u.Id, UserName = u.UserName, Email = u.Email, Roles = roles });
            }

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // Export users CSV
        public async Task<IActionResult> ExportUsers()
        {
            var users = _userManager.Users.ToList();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Id,UserName,Email,Roles,LockoutEnd");
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var rolesCsv = string.Join("|", roles);
                var lockout = u.LockoutEnd?.ToString() ?? string.Empty;
                sb.AppendLine($"\"{u.Id}\",\"{u.UserName}\",\"{u.Email}\",\"{rolesCsv}\",\"{lockout}\"");
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "users_export.csv");
        }

        // View user details
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            var model = new AdminUserViewModel { Id = user.Id, UserName = user.UserName, Email = user.Email, Roles = roles };
            ViewBag.LockoutEnd = user.LockoutEnd;

            // load recent audit logs for this user (most recent 20)
            var logs = await _db.AuditLogs
                .Where(a => a.TargetUserId == id)
                .OrderByDescending(a => a.Timestamp)
                .Take(20)
                .ToListAsync();
            model.AuditLogs = logs;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(string id, string? reason)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            // server-side validation for reason length
            if (!string.IsNullOrEmpty(reason) && reason.Length > 500)
            {
                TempData["Error"] = "Deactivate reason must be 500 characters or fewer.";
                return RedirectToAction("Details", new { id });
            }
            user.LockoutEnd = DateTimeOffset.MaxValue;
            await _userManager.UpdateAsync(user);
            // record audit log for role changes
            var log = new AuditLog
            {
                ActionType = "DeactivateUser",
                PerformedBy = User?.Identity?.Name ?? string.Empty,
                TargetUserId = id,
                Reason = reason,
                Details = null,
                Timestamp = DateTime.UtcNow
            };
            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Reactivate(string id, string? reason)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            // server-side validation for reason length
            if (!string.IsNullOrEmpty(reason) && reason.Length > 500)
            {
                TempData["Error"] = "Reactivate reason must be 500 characters or fewer.";
                return RedirectToAction("Details", new { id });
            }
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);
            var log = new AuditLog
            {
                ActionType = "ReactivateUser",
                PerformedBy = User?.Identity?.Name ?? string.Empty,
                TargetUserId = id,
                Reason = reason ?? "",
                Details = null,
                Timestamp = DateTime.UtcNow
            };
            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
            TempData["Success"] = "User reactivated";
            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> EditRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new EditRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = new List<RoleCheckbox>()
            };

            foreach (var role in _roleManager.Roles)
            {
                model.Roles.Add(new RoleCheckbox
                {
                    RoleName = role.Name,
                    Selected = await _userManager.IsInRoleAsync(user, role.Name)
                });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRoles(EditRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles.Where(r => r.Selected).Select(r => r.RoleName).ToArray();

            var removeRes = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRes.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove existing roles");
                return View(model);
            }

            var addRes = await _userManager.AddToRolesAsync(user, selectedRoles);
            if (!addRes.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add selected roles");
                return View(model);
            }

            // record audit log for role changes
            var removed = currentRoles.Except(selectedRoles).ToArray();
            var added = selectedRoles.Except(currentRoles).ToArray();
            var details = $"Added: {string.Join(',', added)}; Removed: {string.Join(',', removed)}";
            var roleLog = new AuditLog
            {
                ActionType = "EditRoles",
                PerformedBy = User?.Identity?.Name ?? string.Empty,
                TargetUserId = user.Id,
                Reason = null,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
            _db.AuditLogs.Add(roleLog);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Roles updated";

            return RedirectToAction("Index");
        }

        // List audit logs with simple paging/filtering
        public async Task<IActionResult> AuditLogs(string? q, int page = 1)
        {
            const int pageSize = 20;
            var query = _db.AuditLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(a => a.TargetUserId.Contains(q) || a.ActionType.Contains(q) || a.PerformedBy.Contains(q) || (a.Reason != null && a.Reason.Contains(q)) || (a.Details != null && a.Details.Contains(q)));
            }

            var total = await query.CountAsync();
            var totalPages = (int)System.Math.Ceiling(total / (double)pageSize);
            var items = await query.OrderByDescending(a => a.Timestamp).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new AuditLogsListViewModel
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                Query = q
            };

            return View(vm);
        }

        // Export audit logs CSV (respects optional filter q)
        public async Task<IActionResult> ExportAuditLogs(string? q)
        {
            var query = _db.AuditLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(a => a.TargetUserId.Contains(q) || a.ActionType.Contains(q) || a.PerformedBy.Contains(q) || (a.Reason != null && a.Reason.Contains(q)) || (a.Details != null && a.Details.Contains(q)));
            }

            var logs = await query.OrderByDescending(a => a.Timestamp).ToListAsync();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Timestamp,ActionType,PerformedBy,TargetUserId,Reason,Details");
            foreach (var l in logs)
            {
                var ts = l.Timestamp.ToString("o");
                sb.AppendLine($"\"{ts}\",\"{l.ActionType}\",\"{l.PerformedBy}\",\"{l.TargetUserId}\",\"{(l.Reason ?? string.Empty)}\",\"{(l.Details ?? string.Empty)}\"");
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = string.IsNullOrWhiteSpace(q) ? "auditlogs.csv" : $"auditlogs_filtered_{System.Net.WebUtility.UrlEncode(q)}.csv";
            return File(bytes, "text/csv", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> Deactivate(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View("Deactivate", user.UserName);
        }
    }

    // View models
    public class AdminUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
        // recent audit logs for this user (populated on Details view)
        public List<UbuhlebethuConnectPro.Web.Models.AuditLog> AuditLogs { get; set; } = new List<UbuhlebethuConnectPro.Web.Models.AuditLog>();
    }

    public class EditRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<RoleCheckbox> Roles { get; set; }
    }

    public class RoleCheckbox
    {
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }
}
