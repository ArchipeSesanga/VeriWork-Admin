using VeriWork_Admin.Application.Models;
using VeriWork_Admin.Core.Entities;
using VeriWork_Admin.Core.Interfaces;
using BCrypt.Net;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;

namespace VeriWork_Admin.Application.Services
{
    public class AdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly FirestoreDb _db;
        private readonly AuditLogService _auditLogService;

        public AdminService(IUserRepository userRepository, FirestoreDb db,  AuditLogService auditLogService)
        {
            _userRepository = userRepository;
            _db = db;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Registers a new user in Firebase Authentication and Firestore.
        /// Admins are also stored in the 'Admins' collection.
        /// </summary>
       public async Task Register(RegistrationModel model, string? photoUrl, string role = "employee")
{
    // Hash password for security
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

    // Build full user entity
    var user = new User
    {
        IdNumber = model.IdNumber,
        Name = model.Name,
        Surname = model.Surname,
        Phone = model.Phone,
        Address = model.Address,
        City = model.City,
        Province = model.Province,
        Country = model.Country,
        PostalCode = model.PostalCode,
        Gender = model.Gender,
        EmergencyName = model.EmergencyName,
        EmergencyPhone = model.EmergencyPhone,
        HireDate = model.HireDate, 
        Email = model.Email,
        PasswordHash = hashedPassword,
        Role = model.Role ?? role,
        DepartmentId = model.DepartmentId,
        PhotoUrl = photoUrl,
        Position = model.Position,
        DocumentUrls = model.DocumentUrls ?? new List<string>()
    };

    // ✅ Create Firebase Authentication user
    try
    {
        var userRecordArgs = new UserRecordArgs
        {
            Email = model.Email,
            Password = model.Password,
            DisplayName = $"{model.Name} {model.Surname}",
            Disabled = false
        };

        await FirebaseAuth.DefaultInstance.CreateUserAsync(userRecordArgs);
    }
    catch (FirebaseAuthException ex)
    {
        Console.WriteLine($"[Firebase Auth] Error creating user: {ex.Message}");
        throw new Exception("Failed to create Firebase authentication user.");
    }

    // ✅ Save user to Firestore "Users" collection
    await _userRepository.AddUserAsync(user);

    // ✅ If the user is an Admin, also store in "Admins" collection
    if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
    {
        var adminCollection = _db.Collection("Admins");
        await adminCollection.Document(user.IdNumber).SetAsync(user);
    }
}


        /// <summary>
        /// Authenticates an admin by verifying their credentials.
        /// </summary>
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    return user;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a user profile by ID number.
        /// </summary>
        public async Task<User?> GetProfileAsync(string idNumber)
        {
            return await _userRepository.GetUserByIdAsync(idNumber);
        }

        /// <summary>
        /// Updates an existing user profile in the database.
        /// </summary>
        public async Task UpdateProfileAsync(User updatedUser)
        {
            await _userRepository.UpdateUserAsync(updatedUser);
        }

        /// <summary>
        /// Fetches all users from the Firestore database.
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task UpdateUserAsync(EditUserModel model)
        {
            if (string.IsNullOrEmpty(model.IdNumber))
                throw new ArgumentException("User ID is required for updating.");

            // ✅ Build the updated user object
            var updatedUser = new User
            {
                IdNumber = model.IdNumber,
                Name = model.Name,
                Surname = model.Surname,
                Phone = model.Phone,
                Address = model.Address,
                City = model.City,
                Province = model.Province,
                PostalCode = model.PostalCode,
                Country = model.Country,
                DepartmentId = model.DepartmentId,
                Gender = model.Gender,
                Role = model.Role,
                Email = model.Email,
                EmergencyName = model.EmergencyName,
                EmergencyPhone = model.EmergencyPhone,
                Position = model.Position,
                HireDate = model.HireDate,
                PhotoUrl = model.PhotoUrl,
                DocumentUrls = model.DocumentUrls ?? new List<string>(),
                VerificationStatus = "Pending"
            };

            // ✅ Update in Firestore
            await _userRepository.UpdateUserAsync(updatedUser);

            // ✅ If Admin, also sync to Admins collection
            if (string.Equals(model.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var adminDoc = _db.Collection("Admins").Document(model.IdNumber);
                await adminDoc.SetAsync(updatedUser, SetOptions.Overwrite);
            }
            await _auditLogService.AddLogAsync(
                "Admin",
                "Update",
                $"Updated user profile: {model.Name} {model.Surname}"
            );
        }
        
        public async Task DeleteUserAsync(string idNumber)
        {
            if (string.IsNullOrEmpty(idNumber))
                throw new ArgumentException("User ID number cannot be null or empty.");

            var userRef = _db.Collection("Users").Document(idNumber);
            var snapshot = await userRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new Exception($"User with ID {idNumber} not found in Firestore.");

            var user = snapshot.ConvertTo<User>();

            // 1️⃣ Delete from Firestore main collection
            await userRef.DeleteAsync();

            // 2️⃣ Delete from Admins collection if exists
            var adminRef = _db.Collection("Admins").Document(idNumber);
            var adminSnapshot = await adminRef.GetSnapshotAsync();
            if (adminSnapshot.Exists)
                await adminRef.DeleteAsync();

            // 3️⃣ Delete from Firebase Authentication (if UID stored)
            try
            {
                if (!string.IsNullOrEmpty(user.Uid))
                {
                    await FirebaseAuth.DefaultInstance.DeleteUserAsync(user.Uid);
                }
                else
                {
                    Console.WriteLine($"⚠️ No UID found for user {user.Email} ({user.IdNumber}). Skipping Firebase Auth delete.");
                }
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine($"⚠️ Firebase Auth delete failed: {ex.Message}");
            }

            // 4️⃣ Log the deletion in audit logs
            await _auditLogService.AddLogAsync(
                "Admin",
                "Delete",
                $"Deleted user: {user.Name} {user.Surname} ({user.Email})"
            );
        }

    }
}
