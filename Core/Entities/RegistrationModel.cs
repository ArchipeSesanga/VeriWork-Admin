using System.ComponentModel.DataAnnotations;
using VeriWork_Admin.Core.Entities;

namespace VeriWork_Admin.Application.Models;

public class RegistrationModel : User
{
    /// <summary>
    /// RegistrationModel is used to capture employee registration data
    /// from the UI. It inherits base user fields and adds registration-specific
    /// properties like Password, ConfirmPassword, and Photo.
    /// </summary>
   
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Profile photo is required")]
        
        public IFormFile Photo { get; set; }
    
    
}