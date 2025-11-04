using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeriWork_Admin.Application.Models;
using VeriWork_Admin.Application.Services;
using VeriWork_Admin.Core.Entities;

namespace VeriWork_Admin.Controllers;

[Authorize]
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
        // if (!ModelState.IsValid)
        //     return View(model);
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

        return RedirectToAction("Dashboard");
    }

    public IActionResult SuccessfulRegistration() => View();
    public IActionResult UnSuccessfulRegistration() => View();

    [HttpGet]
    public async Task<IActionResult> EmployeeProfile(string Uid)
    {
        if (string.IsNullOrEmpty(Uid))
            return BadRequest("UID is required");

        var user = await _adminService.GetProfileAsync(Uid);
        if (user == null)
            return NotFound("User not found");

        return View(user);
    }



    public async Task<IActionResult> ApproveRejectScreen(string uid)
    {
        if (string.IsNullOrEmpty(uid))
            return NotFound();

        var user = await _adminService.GetProfileAsync(uid);
        if (user == null)
            return NotFound();

        return View(user);
    }

    
    [HttpGet]
    public async Task<IActionResult> Edit(string uid)
    {
        if (string.IsNullOrEmpty(uid))
            return BadRequest("UID Number is required.");

        var user = await _adminService.GetProfileAsync(uid);
        if (user == null)
            return NotFound();

        var model = new EditUserModel
        {
            Uid = user.Uid,
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
            DocumentUrls = user.DocumentUrls,
            VerificationNotes = user.VerificationNotes,
        };

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userRef = _db.Collection("Users").Document(model.Uid);
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
        user.VerificationNotes = model.VerificationNotes ?? user.VerificationNotes;
        user.VerificationStatus = model.VerificationStatus ?? user.VerificationStatus;

        await userRef.SetAsync(user, SetOptions.Overwrite);

        await _auditLogService.AddLogAsync(User.Identity?.Name ?? "Unknown Admin", "Update Profile",
            $"Updated profile for: {user.Name} {user.Surname}");

        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction("Dashboard");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string idNumber)
    {
        if (string.IsNullOrEmpty(idNumber))
            return BadRequest("Invalid ID number.");

        try
        {
            await _adminService.DeleteUserAsync(idNumber);
            TempData["SuccessMessage"] = "Employee deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting employee: {ex.Message}";
        }

        return RedirectToAction("Dashboard");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveVerification(string Uid, string? reason)
    {
        if (string.IsNullOrEmpty(Uid))
            return BadRequest("UID is required");

        var user = await _adminService.GetProfileAsync(Uid);
        if (user == null)
            return NotFound("User not found");

        user.VerificationStatus = "Approved"; 
        user.VerificationNotes = reason ?? "Approved by admin";
    
        await _adminService.UpdateProfileAsync(user);

        await _auditLogService.AddLogAsync(
            User.Identity?.Name ?? "Unknown Admin",
            "Verification",
            $"Approved user: {user.Name} {user.Surname}");

        TempData["SuccessMessage"] = $"✅ {user.Name}'s verification has been approved.";
        return RedirectToAction("EmployeeProfile", new { Uid });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectVerification(string Uid, string reason)
    {
        
        if (string.IsNullOrEmpty(Uid))
            return BadRequest("UID is required");

        var user = await _adminService.GetProfileAsync(Uid);
        if (user == null)
            return NotFound("User not found");

        user.VerificationStatus = "Rejected";
        user.VerificationNotes = reason;
        await _adminService.UpdateProfileAsync(user);

        await _auditLogService.AddLogAsync(
            User.Identity?.Name ?? "Unknown Admin",
            "Verification",
            $"Rejected user: {user.Name} {user.Surname} — Reason: {reason}");

        TempData["ErrorMessage"] = $"❌ {user.Name}'s verification has been rejected.";
        return RedirectToAction("EmployeeProfile", new { Uid = user.Uid });
    }

}
