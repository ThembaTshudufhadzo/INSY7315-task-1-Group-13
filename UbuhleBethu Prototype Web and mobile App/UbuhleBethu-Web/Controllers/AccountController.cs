using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace UbuhlebethuConnectPro.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Username and password are required");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Redirect users to role-appropriate landing pages
                var user = await _userManager.FindByNameAsync(username);
                var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

                if (roles.Contains("Client"))
                {
                    return RedirectToAction("ClientPortal", "Dashboard");
                }
                if (roles.Contains("Office"))
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                if (roles.Contains("Management") || roles.Contains("Executive"))
                {
                    return RedirectToAction("Executive", "Dashboard");
                }
                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                // fallback
                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}
