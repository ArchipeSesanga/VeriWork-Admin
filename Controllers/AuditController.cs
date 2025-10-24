using Microsoft.AspNetCore.Mvc;
using VeriWork_Admin.Application.Models;
using VeriWork_Admin.Application.Services;

namespace VeriWork_Admin.Controllers;

public class AuditController : Controller
{
    private readonly AuditLogService _auditLogService;
    private readonly FirebaseStorageService _firebaseStorageService;

    public AuditController(AuditLogService auditLogService, FirebaseStorageService firebaseStorageService)
    {
        _firebaseStorageService = firebaseStorageService;
        _auditLogService = auditLogService;
    }


    public async Task<IActionResult> Index()
    {
        // Fetch all months that have logs
        var auditRoot = _auditLogService.GetRootCollection();
        var monthDocs = await auditRoot.ListDocumentsAsync().ToListAsync();

        var monthlyLogs = new List<MonthlyAuditLogsViewModel>();

        foreach (var monthDoc in monthDocs)
        {
            var month = monthDoc.Id;
            var logs = await _auditLogService.GetLogsByMonthAsync(month);

            monthlyLogs.Add(new MonthlyAuditLogsViewModel
            {
                Month = month,
                Logs = logs,
                PdfUrl = $"https://firebasestorage.googleapis.com/v0/b/veriwork-database.appspot.com/o/audit-reports%2F{Uri.EscapeDataString(month)}%2FAuditLogs_{Uri.EscapeDataString(month)}.pdf?alt=media"
            });
        }

        return View(monthlyLogs);
    }

    [HttpGet]
    public async Task<IActionResult> DownloadMonthlyReport(string month)
    {
        // 1️⃣ Generate PDF
        var pdfPath = await _auditLogService.GenerateMonthlyPdfAsync(month);

        // 2️⃣ Upload to Firebase
        var downloadUrl = await _firebaseStorageService.UploadAuditPdfAsync(pdfPath, month);

        // 3️⃣ Return redirect to Firebase download URL
        return Redirect(downloadUrl);
    }
    
    [HttpGet]
    public async Task<IActionResult> ExportAuditLog(string month)
    {
        var pdfPath = await _auditLogService.GenerateMonthlyPdfAsync(month);
        var pdfUrl = await _firebaseStorageService.UploadAuditPdfAsync(pdfPath, month);

        return File(System.IO.File.ReadAllBytes(pdfPath), "application/pdf", $"AuditLogs_{month}.pdf");
    }

}