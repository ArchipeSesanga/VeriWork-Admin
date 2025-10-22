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
        //Users in our case are Employers 
        var users = await _adminService.GetAllUsersAsync();
        return View(users);  // pass list to the view
    }
    [HttpGet]
    public IActionResult Register() => View();
    
    
    [HttpPost]
    public async Task<IActionResult> Register(RegistrationModel model, IFormFile photo)
    {
        // 1️⃣ Validate form
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

        // 2️⃣ Upload profile photo
        string? photoUrl = null;

        if (model.Photo != null && model.Photo.Length > 0)
        {
            var photoFileName = $"{Guid.NewGuid()}_{model.Photo.FileName}";
            var photoPath = $"uploads/{model.IdNumber}/profile/{photoFileName}";
            photoUrl = await _storageService.UploadFileAsync(model.Photo, photoPath);
        }

        // 3️⃣ Upload documents (if any)
        var documentUrls = new List<string>();
        if (model.Documents != null && model.Documents.Any())
        {
            foreach (var doc in model.Documents)
            {
                if (doc.Length > 0)
                {
                    var docFileName = $"{Guid.NewGuid()}_{doc.FileName}";
                    var docPath = $"uploads/{model.IdNumber}/documents/{docFileName}";
                    var docUrl = await _storageService.UploadFileAsync(doc, docPath);
                    documentUrls.Add(docUrl);
                }
            }
        }

        // Store document URLs in model
        model.DocumentUrls = documentUrls;

        // 4️⃣ Save to Firestore
        await _adminService.Register(model, photoUrl);

        // 5️⃣ If user is Admin → also store in "Admins" collection

        // 6️⃣ Redirect to confirmation page
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

    // Inside your UserController.cs
    public async Task<IActionResult> ApproveRejectScreen(string idNumber)
    {
        if (string.IsNullOrEmpty(idNumber))
        {
            return NotFound();
        }

        // Your logic to find the user by their ID number from the database
        // For example, using Entity Framework:
        // var user = await _context.Users.FirstOrDefaultAsync(u => u.IdNumber == idNumber);
        var user = await _adminService.GetProfileAsync(idNumber);
        

        if (user == null)
        {
            return NotFound();
        }

        return View(user); // Pass the single user object to the view
    }
// Add these two methods to your UserController.cs

[HttpGet]
public async Task<IActionResult> Edit(string idNumber)
{
    if (string.IsNullOrEmpty(idNumber))
    {
        return BadRequest("ID Number is required.");
    }

    // Fetch the existing user data from your service
    var user = await _adminService.GetProfileAsync(idNumber);
    if (user == null)
    {
        return NotFound();
    }

    // Map the user data to your RegistrationModel to pre-populate the form
    var model = new RegistrationModel
    {
        Name = user.Name,
        Surname = user.Surname,
        IdNumber = user.IdNumber,
        DepartmentId = user.DepartmentId,
        Phone = user.Phone,
        Address = user.Address,
        Role = user.Role,
        Position = user.Position,
        Email = user.Email,
        PhotoUrl = user.PhotoUrl // Pass the existing photo URL to the view
    };

    return View(model);
}

    [HttpPost]
    public async Task<IActionResult> Edit(RegistrationModel model)
    {
        if (!ModelState.IsValid)
        {
            // If the form data is invalid, return the view with the entered data
            return View(model);
        }

        // You will need to create an "UpdateUserAsync" method in your AdminService
        // to handle the logic of saving the updated data to Firebase.
        // await _adminService.UpdateUserAsync(model);

        // After successfully updating, redirect to the dashboard or profile page
        return RedirectToAction("Dashboard");
    }

}