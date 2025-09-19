using VeriWork_Admin.Core.Entities;
using System.Threading.Tasks;

namespace VeriWork_Admin.Core.Interfaces;

public interface IUserRepository
{
    Task AddUserAsync(User user);

    Task<User> GetUserByEmailAsync(string email)
    ;


   
    Task UpdateUserAsync(User updatedUser);

    Task<List<User>> GetAllUsersAsync();
    Task<User> GetUserByIdAsync(string idNumber);

}