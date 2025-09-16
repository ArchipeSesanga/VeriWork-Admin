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
        await collection.Document(user.IdNumber).SetAsync(user);
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        var snapshot = await _db.Collection("Users")
            .WhereEqualTo("Email", email)
            .GetSnapshotAsync();

        return snapshot.Documents.FirstOrDefault()?.ConvertTo<User>();
    }

    public Task<User> GetUserByIdAsync(string idNumber)
    {
        throw new NotImplementedException();
    }

    public Task UpdateUserAsync(User updatedUser)
    {
        throw new NotImplementedException();
    }
}