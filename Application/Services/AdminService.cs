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

        public AdminService(IUserRepository userRepository, FirestoreDb db)
        {
            _userRepository = userRepository;
            _db = db;
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
    }
}
