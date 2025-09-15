using System.ComponentModel.DataAnnotations;

namespace VeriWork_Admin.Core.Entities;

public class User
{
    [Key]
    public string IdNumber { get; set; }   // Unique identifier, e.g. SA ID Number

    [Required, StringLength(50)]
    public string Name { get; set; }

    [Required, StringLength(50)]
    public string Surname { get; set; }

    [Required, Phone]
    public string Phone { get; set; }      // Changed from int → string (better for phone numbers)

    [StringLength(200)]
    public string Address { get; set; }

    [Required]
    public string Role { get; set; }       // e.g. "Employee", "Admin"

    [Required, EmailAddress]
    public string Email { get; set; }
    
}