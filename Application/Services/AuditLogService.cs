using Google.Cloud.Firestore;
using VeriWork_Admin.Core.Entities;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Properties;

namespace VeriWork_Admin.Application.Services
{
    public class AuditLogService
    {
        private readonly FirestoreDb _db;

        public AuditLogService()
        {
            _db = FirestoreDb.Create("veriwork-database");
        }

        // ✅ Logs an action to Firestore
        public async Task AddLogAsync(string adminEmail, string actionType, string description)
        {
            var log = new AuditLog
            {
                AdminEmail = adminEmail,
                ActionType = actionType,
                Description = description,
                Month = $"{DateTime.Now:MMMM yyyy}",
                ActionDate = DateTime.UtcNow
            };

            var monthCollection = _db.Collection("AuditLogs")
                                     .Document(log.Month)
                                     .Collection("Logs");

            await monthCollection.AddAsync(log);
        }

        // ✅ Retrieve logs for a specific month
        public async Task<List<AuditLog>> GetLogsByMonthAsync(string month)
        {
            // Fetch logs for the given month, ordered by ActionDate descending
            var snapshot = await _db.Collection("AuditLogs")
                .Document(month)
                .Collection("Logs")
                .OrderByDescending("ActionDate")
                .GetSnapshotAsync();

            // Convert Firestore documents to your AuditLog model
            var logs = snapshot.Documents
                .Select(d => d.ConvertTo<AuditLog>())
                .ToList();

            return logs;
        }
        
        // ✅ Generate PDF Report
        public async Task<string> GenerateMonthlyPdfAsync(string month)
        {
            var logs = await GetLogsByMonthAsync(month);
            var filePath = Path.Combine(Path.GetTempPath(), $"AuditLogs_{month}.pdf");

            using (var writer = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(writer))
            using (var doc = new Document(pdf))
            {
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Header
                var title = new Paragraph($"Audit Log Report - {month}")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10);
                doc.Add(title);

                doc.Add(new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(regularFont)
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.RIGHT));
                doc.Add(new Paragraph("\n"));

                // Table Setup
                var table = new iText.Layout.Element.Table(4).UseAllAvailableWidth();
                table.AddHeaderCell(new Cell().Add(new Paragraph("Date").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Admin Email").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Action").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Description").SetFont(boldFont)));

                // Table Rows
                foreach (var log in logs)
                {
                    table.AddCell(new Cell().Add(new Paragraph(log.ActionDate.ToString("yyyy-MM-dd HH:mm:ss")).SetFont(regularFont)));
                    table.AddCell(new Cell().Add(new Paragraph(log.AdminEmail ?? "").SetFont(regularFont)));
                    table.AddCell(new Cell().Add(new Paragraph(log.ActionType ?? "").SetFont(regularFont)));
                    table.AddCell(new Cell().Add(new Paragraph(log.Description ?? "").SetFont(regularFont)));
                }

                doc.Add(table);
            }

            return filePath;
        }
        public CollectionReference GetRootCollection()
        {
            return _db.Collection("AuditLogs");
        }

    }
}
