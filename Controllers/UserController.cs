using Microsoft.AspNetCore.Mvc;
using VeriWork_Admin.Application.Models;
using VeriWork_Admin.Application.Services;

namespace VeriWork_Admin.Controllers;

public class UserController : Controller
{
    private readonly UserService _userService;
    private readonly FirebaseStorageService _storageService;

    public UserController(UserService userService, FirebaseStorageService storageService)
    {
        _userService = userService;
        _storageService = storageService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Register(RegistrationModel model, IFormFile photo)
    {
        if (!ModelState.IsValid)
            return View(model);

        string photoUrl = null;

        if (photo != null && photo.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}_{photo.FileName}";
            photoUrl = await _storageService.UploadFileAsync(photo, fileName);
        }

        await _userService.RegisterEmployeeAsync(model, photoUrl);

        return RedirectToAction("Login");
    }

    public IActionResult Login()
    {
        return View();
    }
}