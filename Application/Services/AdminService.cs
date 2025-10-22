using VeriWork_Admin.Application.Models;
using VeriWork_Admin.Core.Entities;
using VeriWork_Admin.Core.Interfaces;
using BCrypt.Net;
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
        /// Registers a new user in the Firestore database. 
        /// Admins are saved in both 'Users' and 'Admins' collections.
        /// </summary>
        public async Task Register(RegistrationModel model, string? photoUrl, string role = "employee")
        {
            // Hash password for security
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // Build user object
            var user = new User
            {
                IdNumber = model.IdNumber,
                Name = model.Name,
                Surname = model.Surname,
                Phone = model.Phone,
                Address = model.Address,
                Email = model.Email,
                PasswordHash = hashedPassword,
                Role = model.Role ?? role, // fallback to default role if null
                DepartmentId = model.DepartmentId,
                PhotoUrl = photoUrl,
                Position = model.Position
            };

            // Add optional document URLs if provided
            if (model.DocumentUrls != null && model.DocumentUrls.Any())
            {
                user.DocumentUrls = model.DocumentUrls;
            }

            // Save to "Users" collection (default for all users)
            await _userRepository.AddUserAsync(user);

            // If the role is Admin, also save to "Admins" collection
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
