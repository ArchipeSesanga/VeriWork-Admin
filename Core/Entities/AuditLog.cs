using Google.Cloud.Firestore;

namespace VeriWork_Admin.Core.Entities;

[FirestoreData]
public class AuditLog
{
   [FirestoreProperty]
    public string ActionId { get; set; } = Guid.NewGuid().ToString();
    
    [FirestoreProperty]
    public string AdminEmail { get; set; }
    
    [FirestoreProperty]
    public string ActionType { get; set; } // e.g. "Register", "Edit", "Delete", "Login"

    [FirestoreProperty]
    public string Description { get; set; } // e.g. "Registered new user: John Doe"

    [FirestoreProperty]
    public DateTime ActionDate { get; set; } = DateTime.UtcNow; // always UTC

    [FirestoreProperty]
    public string Month { get; set; } // e.g. "2025-10" (for grouping by month)
    
    
}