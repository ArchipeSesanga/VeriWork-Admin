using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using VeriWork_Admin.Application.Models;
using VeriWork_Admin.Application.Services;
using VeriWork_Admin.Core.Entities;

namespace VeriWork_Admin.Controllers;

public class UserController : Controller
{
    private readonly AdminService _adminService;
    private readonly FirebaseStorageService _storageService;
    private readonly AuditLogService _auditLogService;
    private readonly FirestoreDb _db;

    public UserController(AdminService adminService, FirebaseStorageService storageService, AuditLogService auditLogService, FirestoreDb db)
    {
        _adminService = adminService;
        _storageService = storageService;
        _auditLogService = auditLogService;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var users = await _adminService.GetAllUsersAsync();
        return View(users);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegistrationModel model, IFormFile photo)
    {
        if (!ModelState.IsValid)
            return View(model);

        string? photoUrl = null;

        if (model.Photo != null && model.Photo.Length > 0)
        {
            var photoFileName = $"{Guid.NewGuid()}_{model.Photo.FileName}";
            var photoPath = $"uploads/{model.IdNumber}/profile/{photoFileName}";
            photoUrl = await _storageService.UploadFileAsync(model.Photo, photoPath);
        }

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

        model.DocumentUrls = documentUrls;
        await _adminService.Register(model, photoUrl);

        await _auditLogService.AddLogAsync(User.Identity?.Name ?? "Unknown Admin", "Register",
            $"Registered new user: {model.Name} {model.Surname}");

        return RedirectToAction("SuccessfulRegistration");
    }

    public IActionResult SuccessfulRegistration() => View();
    public IActionResult UnSuccessfulRegistration() => View();

    public async Task<IActionResult> EmployeeProfile(string idNumber)
    {
        if (string.IsNullOrEmpty(idNumber))
            return BadRequest("ID Number is required");

        var user = await _adminService.GetProfileAsync(idNumber);
        if (user == null)
            return NotFound();

        return View(user);
    }

    public async Task<IActionResult> ApproveRejectScreen(string idNumber)
    {
        if (string.IsNullOrEmpty(idNumber))
            return NotFound();

        var user = await _adminService.GetProfileAsync(idNumber);
        if (user == null)
            return NotFound();

        return View(user);
    }

    // ✅ GET: Edit
    [HttpGet]
    public async Task<IActionResult> Edit(string idNumber)
    {
        if (string.IsNullOrEmpty(idNumber))
            return BadRequest("ID Number is required.");

        var user = await _adminService.GetProfileAsync(idNumber);
        if (user == null)
            return NotFound();

        var model = new EditUserModel
        {
            IdNumber = user.IdNumber,
            Name = user.Name,
            Surname = user.Surname,
            DepartmentId = user.DepartmentId,
            HireDate = user.HireDate,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            EmergencyName = user.EmergencyName,
            EmergencyPhone = user.EmergencyPhone,
            City = user.City,
            Province = user.Province,
            PostalCode = user.PostalCode,
            Country = user.Country,
            Position = user.Position,
            Gender = user.Gender,
            Email = user.Email,
            PhotoUrl = user.PhotoUrl,
            DocumentUrls = user.DocumentUrls
        };

        return View(model);
    }

    // ✅ POST: Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userRef = _db.Collection("Users").Document(model.IdNumber);
        var snapshot = await userRef.GetSnapshotAsync();
        if (!snapshot.Exists)
            return NotFound("User not found");

        var user = snapshot.ConvertTo<User>();

        // Upload new profile photo
        if (model.Photo != null && model.Photo.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}_{model.Photo.FileName}";
            user.PhotoUrl = await _storageService.UploadFileAsync(model.Photo, fileName);
        }

        // Upload new documents
        if (model.Documents != null && model.Documents.Any())
        {
            user.DocumentUrls ??= new List<string>();
            foreach (var doc in model.Documents)
            {
                var fileName = $"{Guid.NewGuid()}_{doc.FileName}";
                var url = await _storageService.UploadFileAsync(doc, fileName);
                user.DocumentUrls.Add(url);
            }
        }

        // Update fields
        user.Name = model.Name ?? user.Name;
        user.Surname = model.Surname ?? user.Surname;
        user.Phone = model.Phone ?? user.Phone;
        user.Address = model.Address ?? user.Address;
        user.City = model.City ?? user.City;
        user.Province = model.Province ?? user.Province;
        user.PostalCode = model.PostalCode ?? user.PostalCode;
        user.Country = model.Country ?? user.Country;
        user.DepartmentId = model.DepartmentId ?? user.DepartmentId;
        user.Position = model.Position ?? user.Position;
        user.Role = model.Role ?? user.Role;
        user.EmergencyName = model.EmergencyName ?? user.EmergencyName;
        user.EmergencyPhone = model.EmergencyPhone ?? user.EmergencyPhone;
        user.HireDate = model.HireDate ?? user.HireDate;
        user.Gender = model.Gender ?? user.Gender;
        user.Email = model.Email ?? user.Email;

        await userRef.SetAsync(user, SetOptions.Overwrite);

        await _auditLogService.AddLogAsync(User.Identity?.Name ?? "Unknown Admin", "Update Profile",
            $"Updated profile for: {user.Name} {user.Surname}");

        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction("Dashboard");
    }
}
