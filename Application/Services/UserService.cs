using VeriWork_Admin.Application.Mappers;
using VeriWork_Admin.Application.Models;
using VeriWork_Admin.Core.Entities;
using VeriWork_Admin.Core.Interfaces;

namespace VeriWork_Admin.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public async Task RegisterEmployeeAsync(RegistrationModel model, string photoUrl)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

        var employee = model.ToEmployee(photoUrl, hashedPassword);

        await _userRepository.AddUserAsync(employee);
    }
    public async Task<User> AuthenticateAsync(string email, string password)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);

        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return user;

        return null;
    }
    public async Task<User> GetProfileAsync(string idNumber)
    {
        return await _userRepository.GetUserByIdAsync(idNumber);
    }
    
    public async Task UpdateProfileAsync(User updatedUser)
    {
        await _userRepository.UpdateUserAsync(updatedUser);
    }



}