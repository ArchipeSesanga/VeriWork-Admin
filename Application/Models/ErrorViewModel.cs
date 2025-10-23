namespace VeriWork_Admin.Application.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    
    public int code {get;set;}
    public string message { get; set; }
 
}