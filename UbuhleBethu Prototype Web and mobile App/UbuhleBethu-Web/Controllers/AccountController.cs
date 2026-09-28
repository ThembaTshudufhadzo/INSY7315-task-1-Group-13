using QRCoder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace UbuhlebethuConnectPro.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // =========================================================
        // LOGIN
        // =========================================================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string username,
            string password,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Username and password are required.");

                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                username,
                password,
                isPersistent: false,
                lockoutOnFailure: true);

            // =====================================================
            // MFA ALREADY ENABLED
            // =====================================================

            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(
                    nameof(VerifyTwoFactor),
                    new { returnUrl });
            }

            // =====================================================
            // PASSWORD LOGIN SUCCESSFUL
            // =====================================================

            if (result.Succeeded)
            {
                var user =
                    await _userManager.FindByNameAsync(username);

                if (user == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to load your account.");

                    return View();
                }

                // =================================================
                // MFA NOT YET CONFIGURED
                // =================================================

                if (!user.TwoFactorEnabled)
                {
                    return RedirectToAction(
                        nameof(SetupMfa),
                        new { returnUrl });
                }

                // =================================================
                // MFA ALREADY CONFIGURED
                // =================================================

                return await RedirectAfterLogin(
                    username,
                    returnUrl);
            }

            // =====================================================
            // ACCOUNT LOCKED
            // =====================================================

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account is temporarily locked. Please try again later.");

                return View();
            }

            // =====================================================
            // INVALID LOGIN
            // =====================================================

            ModelState.AddModelError(
                string.Empty,
                "Invalid username or password.");

            return View();
        }

        // =========================================================
        // TWO-FACTOR AUTHENTICATION LOGIN
        // =========================================================

        [HttpGet]
        public IActionResult VerifyTwoFactor(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTwoFactor(
            string code,
            bool rememberMachine,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please enter the 6-digit verification code from your authenticator app.");

                return View();
            }

            var user =
                await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your authentication session has expired. Please log in again.");

                return View();
            }

            // Remove spaces and hyphens.
            code = code
                .Replace(" ", "")
                .Replace("-", "");

            var result =
                await _signInManager.TwoFactorAuthenticatorSignInAsync(
                    code,
                    isPersistent: false,
                    rememberClient: rememberMachine);

            if (result.Succeeded)
            {
                if (string.IsNullOrEmpty(user.UserName))
                {
                    return RedirectToAction(nameof(Login));
                }

                return await RedirectAfterLogin(
                    user.UserName,
                    returnUrl);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account is temporarily locked because of too many unsuccessful verification attempts.");

                return View();
            }

            ModelState.AddModelError(
                string.Empty,
                "The verification code is incorrect or has expired. Please enter the current 6-digit code from your authenticator app.");

            return View();
        }

        // =========================================================
        // MFA SETUP
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> SetupMfa(
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // =====================================================
            // IF MFA IS ALREADY ENABLED
            // =====================================================

            if (user.TwoFactorEnabled)
            {
                return RedirectToAction(
                    nameof(VerifyTwoFactor),
                    new { returnUrl });
            }

            // =====================================================
            // GENERATE AUTHENTICATOR KEY
            // =====================================================

            var key =
                await _userManager.GetAuthenticatorKeyAsync(user);

            if (string.IsNullOrEmpty(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);

                key =
                    await _userManager.GetAuthenticatorKeyAsync(user);
            }

            if (string.IsNullOrEmpty(key))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to generate an authenticator key.");

                return View();
            }

            var email =
                await _userManager.GetEmailAsync(user)
                ?? user.UserName
                ?? "user";

            const string issuer =
                "Ubuhlebethu ConnectPro";

            // =====================================================
            // CREATE AUTHENTICATOR URI
            // =====================================================

            var authenticatorUri =
                $"otpauth://totp/" +
                $"{Uri.EscapeDataString(issuer)}:" +
                $"{Uri.EscapeDataString(email)}" +
                $"?secret={key}" +
                $"&issuer={Uri.EscapeDataString(issuer)}" +
                "&digits=6" +
                "&period=30";

            // =====================================================
            // GENERATE QR CODE
            // =====================================================

            using var qrGenerator =
                new QRCodeGenerator();

            using var qrCodeData =
                qrGenerator.CreateQrCode(
                    authenticatorUri,
                    QRCodeGenerator.ECCLevel.Q);

            var qrCode =
                new PngByteQRCode(qrCodeData);

            var qrBytes =
                qrCode.GetGraphic(6);

            ViewData["AuthenticatorKey"] =
                key;

            ViewData["QrCodeImage"] =
                Convert.ToBase64String(qrBytes);

            return View();
        }

        // =========================================================
        // ENABLE MFA
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableMfa(
            string code,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // =====================================================
            // VALIDATE CODE
            // =====================================================

            if (string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please enter the 6-digit verification code from your authenticator app.");

                return await SetupMfa(returnUrl);
            }

            // Remove spaces and hyphens.
            code = code
                .Replace(" ", "")
                .Replace("-", "");

            if (code.Length != 6 ||
                !code.All(char.IsDigit))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The verification code must contain 6 digits.");

                return await SetupMfa(returnUrl);
            }

            // =====================================================
            // VERIFY AUTHENTICATOR CODE
            // =====================================================

            var isValid =
                await _userManager.VerifyTwoFactorTokenAsync(
                    user,
                    _userManager.Options
                        .Tokens
                        .AuthenticatorTokenProvider,
                    code);

            if (!isValid)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The verification code is incorrect or has expired. Please enter the current 6-digit code from your authenticator app.");

                return await SetupMfa(returnUrl);
            }

            // =====================================================
            // ENABLE MFA
            // =====================================================

            var result =
                await _userManager.SetTwoFactorEnabledAsync(
                    user,
                    true);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return await SetupMfa(returnUrl);
            }

            // =====================================================
            // MFA SUCCESS
            // =====================================================

            return RedirectToAction(
                nameof(MfaEnabled),
                new { returnUrl });
        }

        // =========================================================
        // MFA SUCCESS
        // =========================================================

        [HttpGet]
        public IActionResult MfaEnabled(
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        // =========================================================
        // LOGOUT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                nameof(Login),
                "Account");
        }

        // =========================================================
        // ACCESS DENIED
        // =========================================================

        [HttpGet]
        public IActionResult AccessDenied(
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        // =========================================================
        // ROLE-BASED REDIRECT
        // =========================================================

        private async Task<IActionResult> RedirectAfterLogin(
            string username,
            string? returnUrl)
        {
            // If the user originally requested a specific
            // local page, send them there after authentication.
            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var user =
                await _userManager.FindByNameAsync(username);

            if (user != null)
            {
                var roles =
                    await _userManager.GetRolesAsync(user);

                if (roles.Contains("Client"))
                {
                    return RedirectToAction(
                        "ClientPortal",
                        "Dashboard");
                }

                if (roles.Contains("Office"))
                {
                    return RedirectToAction(
                        "Index",
                        "Dashboard");
                }

                if (roles.Contains("Management") ||
                    roles.Contains("Executive"))
                {
                    return RedirectToAction(
                        "Executive",
                        "Dashboard");
                }

                if (roles.Contains("Admin"))
                {
                    return RedirectToAction(
                        "Index",
                        "Admin");
                }
            }

            return RedirectToAction(
                nameof(Login),
                "Account");
        }
    }
}