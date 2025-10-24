using System.Security.Claims;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using VeriWork_Admin.Application.Models;
using VeriWork_Admin.Core.Entities;
using VeriWork_Admin.Application.Services;

namespace VeriWork_Admin.Controllers
{
    public class AuthController : Controller
    {
        private readonly FirebaseAuthService _authService;
        private readonly AuditLogService _auditLogService;

        // ✅ Inject both services
        public AuthController(FirebaseAuthService authService, AuditLogService auditLogService)
        {
            _authService = authService;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authService.AuthenticateAsync(model.Email, model.Password);

            if (user == null)
            {
                ViewBag.Error = "Invalid credentials or user not found.";
                return View(model);
            }

            // Optional: Admin-only access
            if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Access denied. Admins only.";
                return View(model);
            }

            // ✅ Store session info
            HttpContext.Session.SetString("AdminEmail", user.Email);
            HttpContext.Session.SetString("AdminName", $"{user.Name} {user.Surname}");

            // ✅ Retrieve Firebase user
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(model.Email);
            if (userRecord == null)
            {
                ViewBag.Error = "Invalid credentials or user not found.";
                return View(model);
            }

            // ✅ Create cookie claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userRecord.DisplayName ?? userRecord.Email),
                new Claim(ClaimTypes.Email, userRecord.Email),
                new Claim("uid", userRecord.Uid)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
                });

            // ✅ Add audit log
            await _auditLogService.AddLogAsync(
                userRecord.Email,
                "Login",
                $"{userRecord.DisplayName ?? userRecord.Email} logged in."
            );

            // ✅ Redirect to dashboard
            return RedirectToAction("Dashboard", "User");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Auth");
        }
    }
}
