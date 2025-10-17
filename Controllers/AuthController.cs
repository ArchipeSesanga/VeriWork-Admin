using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using VeriWork_Admin.Application.Services;
using VeriWork_Admin.Core.Entities;

namespace VeriWork_Admin.Controllers;

public class AuthController : Controller
{
    private readonly AdminService _authService;

    public AuthController(AdminService authService)
    {
        _authService = authService;
    }

    [HttpGet] //Display the login View
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var admin = await _authService.AuthenticateAsync(model.Email, model.Password); //returns null when credentials are invalid

        if (admin != null) //if admin is fine redirects to Dashboard. 
        {
            // ✅ Save login in session/cookie
            HttpContext.Session.SetString("AdminEmail", admin.Email);
            HttpContext.Session.SetString("AdminName", admin.Name);
            return RedirectToAction("Dashboard", "User");
        }

        ModelState.AddModelError("", "Invalid credentials or not authorized.");
        return View(model);
    }
}
    