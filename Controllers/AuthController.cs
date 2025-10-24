using Microsoft.AspNetCore.Mvc;
using VeriWork_Admin.Application.Services;

public class AuthController : Controller
{
    private readonly FirebaseAuthService _authService;

    public AuthController(FirebaseAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _authService.AuthenticateAsync(email, password);

        if (user == null)
        {
            ViewBag.Error = "Invalid credentials or user not found.";
            return View();
        }

        // Optional: check for Admin role
        if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.Error = "Access denied. Admins only.";
            return View();
        }

        // Store user in session
        HttpContext.Session.SetString("AdminEmail", user.Email);
        HttpContext.Session.SetString("AdminName", $"{user.Name} {user.Surname}");

        return RedirectToAction("Dashboard", "User");
    }
}