using VeriWork_Admin.Core.Entities;

namespace VeriWork_Admin.Application.Models;

public class MonthlyAuditLogsViewModel
{
   
        /// <summary>
        /// The month of the logs (e.g., "October 2025")
        /// </summary>
        public string Month { get; set; } = string.Empty;

        /// <summary>
        /// List of audit log entries for this month
        /// </summary>
        public List<AuditLog> Logs { get; set; } = new();

        /// <summary>
        /// The URL of the monthly PDF file stored in Firebase (optional)
        /// </summary>
        public string? PdfUrl { get; set; }
    
}