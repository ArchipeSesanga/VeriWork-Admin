using Microsoft.AspNetCore.Mvc;
using VeriWork_Admin.Application.Models;
using VeriWork_Admin.Application.Services;

namespace VeriWork_Admin.Controllers;

public class UserController : Controller
{
    private readonly AdminService _adminService;
    private readonly FirebaseStorageService _storageService;

    public UserController(AdminService adminService, FirebaseStorageService storageService)
    {
        _adminService = adminService;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        //Use: Display dashboard with list of users
        var users = await _adminService.GetAllUsersAsync();
        return View(users);  // pass list to the view
    }
    [HttpGet]
    public IActionResult Register()
    {
        //Use: Display the registration page
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Register(RegistrationModel model, IFormFile photo)
    {
        //Use: Register new employee into Firebase database
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(kv => kv.Value.Errors.Count > 0)
                .SelectMany(kv => kv.Value.Errors.Select(e => $"{kv.Key}: {e.ErrorMessage}"))
                .ToList();

            foreach (var error in errors)
                Console.WriteLine(error);

            return View(model);
        }

        string? photoUrl = null;

        if (model.Photo != null && model.Photo.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}_{photo.FileName}";
            photoUrl = await _storageService.UploadFileAsync(photo, fileName);
        }

        await _adminService.Register(model, photoUrl);

        return RedirectToAction("SuccessfulRegistration");
    }
    
    public IActionResult Login()
    {
        return View();
    }
    
    

    
    
    
    public IActionResult SuccessfulRegistration()
    {
        //testing purpose
        return View();
    }
    
    public IActionResult UnSuccessfulRegistration()
    {
        //testing purpose
        return View();
    }

    public async Task<IActionResult> EmployeeProfile(string idNumber)
    {
        if (string.IsNullOrEmpty(idNumber))
            return BadRequest("ID Number is required");
        
        var user = await _adminService.GetProfileAsync(idNumber);
        if (user == null)
            return NotFound();
        return View(user);
    }
}