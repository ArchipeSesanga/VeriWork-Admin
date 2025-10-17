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
        /// Register new user in the database. 
        /// Admins are saved in both 'Users' and 'Admins' collections.
        /// </summary>
        public async Task Register(RegistrationModel model, string? photoUrl, string role = "employee")
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                IdNumber = model.IdNumber,
                Name = model.Name,
                Surname = model.Surname,
                Phone = model.Phone,
                Address = model.Address,
                Email = model.Email,
                PasswordHash = hashedPassword,
                Role = model.Role, // Use Role from model instead of fixed parameter
                DepartmentId = model.DepartmentId,
                PhotoUrl = photoUrl,
                Position = model.Position
            };

            // Save in "Users" collection (every registered person)
            await _userRepository.AddUserAsync(user);

            // If the role is Admin, also save in "Admins" collection
            if (model.Role != null && model.Role.ToLower() == "admin")
            {
                var adminCollection = _db.Collection("Admins");
                await adminCollection.Document(model.IdNumber).SetAsync(user);
            }
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                if (user.Role.ToLower() == "admin")
                    return user;
            }

            return null;
        }

        public async Task<User?> GetProfileAsync(string idNumber)
        {
            return await _userRepository.GetUserByIdAsync(idNumber);
        }

        public async Task UpdateProfileAsync(User updatedUser)
        {
            await _userRepository.UpdateUserAsync(updatedUser);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }
    }
}
