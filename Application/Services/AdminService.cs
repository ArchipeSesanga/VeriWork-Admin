using VeriWork_Admin.Application.Mappers;
using VeriWork_Admin.Application.Models;
using VeriWork_Admin.Core.Entities;
using VeriWork_Admin.Core.Interfaces;

namespace VeriWork_Admin.Application.Services;

public class AdminService
{
    private readonly IUserRepository _userRepository;

    public AdminService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Register(RegistrationModel model, string photoUrl, string role = "employee")
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
            Role = role,
            DepartmentId = role == "employee" ? model.DepartmentId : null,
            
        };

        await _userRepository.AddUserAsync(user);
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);

        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return user;

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
}