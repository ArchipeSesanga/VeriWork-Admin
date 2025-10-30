using Google.Cloud.Firestore;
using VeriWork_Admin.Core.Entities;
using VeriWork_Admin.Core.Interfaces;


namespace VeriWork_Admin.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly FirestoreDb _db;

    public UserRepository(FirestoreDb db)
    {
        _db = db;
    }

    public async Task AddUserAsync(User user)
    {
        var collection = _db.Collection("Users");
        await collection.Document(user.Uid).SetAsync(user);
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        var snapshot = await _db.Collection("Users")
            .WhereEqualTo("Email", email)
            .GetSnapshotAsync();

        return snapshot.Documents.FirstOrDefault()?.ConvertTo<User>();
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        var users = new List<User>();
        var snapshot = await _db.Collection("Users").GetSnapshotAsync();
        foreach (var doc in snapshot.Documents)
        {
            if (doc.Exists)
            {
                Console.WriteLine($"Doc {doc.Id}: {doc.ToDictionary()}");
                var user = doc.ConvertTo<User>();
                users.Add(user);
            }
        }
        return users;
    }

    public async Task<User> GetUserByIdAsync(string idNumber)
    {
       
        var docRef = _db.Collection("Users").Document(idNumber);
        var snapshot = await docRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            return snapshot.ConvertTo<User>();
        }
        return snapshot.Exists ? snapshot.ConvertTo<User>() : null;
    }

    public async Task UpdateUserAsync(User updatedUser)
    {
        if (string.IsNullOrEmpty(updatedUser.Uid))
            throw new ArgumentException("User IdNumber cannot be null or empty.");

        // 🔥 Ensure you are referencing the correct Firestore document
        var userRef = _db.Collection("Users").Document(updatedUser.Uid);
        var snapshot = await userRef.GetSnapshotAsync();

        if (!snapshot.Exists)
            throw new Exception($"User with ID {updatedUser.Uid} not found in Firestore.");

        // ✅ Build update dictionary
        var updates = new Dictionary<string, object>
        {
            { "VerificationStatus", updatedUser.VerificationStatus },
            { "VerificationNotes", updatedUser.VerificationNotes },
            { "LastModified", Timestamp.FromDateTime(DateTime.UtcNow) } // optional audit field
        };

        await userRef.UpdateAsync(updates);
    }

}