using System.ComponentModel.DataAnnotations;

namespace VeriWork_Admin.Application.Models;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }
    
    [Required]
    public string Password { get; set; }
}