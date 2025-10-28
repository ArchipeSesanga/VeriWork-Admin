using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace VeriWork_Admin.Core.Entities;

[FirestoreData]  
public class User
{
    [Key]
    [FirestoreProperty]
    public string IdNumber { get; set; }  // Unique identifier, e.g. SA ID Number

    [FirestoreProperty]
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    public string Name { get; set; }

    [FirestoreProperty]
    [Required(ErrorMessage = "Surname is required.")]
    [StringLength(50, ErrorMessage = "Surname cannot exceed 50 characters.")]
    public string Surname { get; set; }

    [FirestoreProperty]
    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string Phone { get; set; }
    
    [FirestoreProperty]
    [Required(ErrorMessage = "Emergency name  is required.")]
    [StringLength(50, ErrorMessage = "name cannot exceed 50 characters.")]
    public string EmergencyName { get; set; }
   
    [FirestoreProperty]
    [Required(ErrorMessage = "Emergency phone number is required.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string EmergencyPhone { get; set; }

    [FirestoreProperty]
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
    public string Address { get; set; }
    
    [FirestoreProperty]
    [Required(ErrorMessage = "City is required.")]
    [StringLength(200, ErrorMessage = "City cannot exceed 50 characters.")]
    public string City { get; set; }
    
    [FirestoreProperty]
    [Required(ErrorMessage = "Province is required.")]
    [StringLength(200, ErrorMessage = "Province cannot exceed 50 characters.")]
    public string Province { get; set; }
    
    [FirestoreProperty]
    public string PostalCode { get; set; }
    
    
    [FirestoreProperty]
    [Required(ErrorMessage = "Country is required.")]
    public string Country { get; set; }
    

    [FirestoreProperty]
    [Required(ErrorMessage = "Department ID is required.")]
    public string DepartmentId { get; set; }
    
    [FirestoreProperty]
    [Required(ErrorMessage = "Gender is required.")]
    public string Gender { get; set; }

    [FirestoreProperty]
    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; }

    [FirestoreProperty]
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; }

    [FirestoreProperty]
    public string? PasswordHash { get; set; }

    [FirestoreProperty]
    public string? PhotoUrl { get; set; }

    [FirestoreProperty]
    [Required(ErrorMessage = "Position is required.")]
    [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters.")]
    public string Position { get; set; }
    
    [FirestoreProperty]
    [Required(ErrorMessage = "Date is required.")]
    public String HireDate { get; set; }
    [FirestoreProperty]
    public List<string>? DocumentUrls { get; set; } = new List<string>();
    
    [FirestoreProperty]
    public string? Uid { get; set; } // For firebase Authentication
    
    [FirestoreProperty]
    public string VerificationStatus { get; set; } = "Pending";
    
    [FirestoreProperty]
    public string? VerificationNotes { get; set; } 


    
    
}