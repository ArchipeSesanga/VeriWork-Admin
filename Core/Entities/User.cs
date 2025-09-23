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
    [Required, StringLength(50)]
    public string Name { get; set; }

    [FirestoreProperty]
    [Required, StringLength(50)]
    public string Surname { get; set; }

    [FirestoreProperty]
    [Required, Phone]
    public string Phone { get; set; }      // Changed from int → string (better for phone numbers)

    [FirestoreProperty]
    [StringLength(200)]
    public string Address { get; set; }
    
   
    [FirestoreProperty]
    public string DepartmentId { get; set; }
    
    [Required]
    [FirestoreProperty]
    public string Role { get; set; }       // e.g. "Employee", "Admin"

    [Required, EmailAddress]
    [FirestoreProperty]
    public string Email { get; set; }
    
    [FirestoreProperty]
    public string? PasswordHash { get; set; }
    
    [FirestoreProperty]
    public string? PhotoUrl { get; set; }
}