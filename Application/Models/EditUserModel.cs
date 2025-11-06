using System.ComponentModel.DataAnnotations;

namespace VeriWork_Admin.Application.Models;

public class EditUserModel
{
    public string? Uid { get; set; }
    public string? IdNumber { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? DepartmentId { get; set; }
    public string? Position { get; set; }
    public string? Role { get; set; }
    public string? EmergencyName { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? HireDate { get; set; }
    public string? Gender { get; set; }
    public string? Email { get; set; }
    
    public string? VerificationStatus { get; set; }
    public string? VerificationNotes { get; set; }
    // For file uploads
    public IFormFile? Photo { get; set; }
    public List<IFormFile>? Documents { get; set; }

    // The existing URLs (optional)
    public string? PhotoUrl { get; set; }
    public List<string>? DocumentUrls { get; set; }
    public string? SelfieUrl {get; set;} 
}
