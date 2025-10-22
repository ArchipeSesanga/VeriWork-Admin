namespace VeriWork_Admin.Core.Entities;

public class AuditLogViewModel
{
    public string Month { get; set; } = "";
    public List<AuditLog> Logs { get; set; } = new();
    public string? PdfUrl { get; set; }  // URL of uploaded monthly PDF in Firebase
}