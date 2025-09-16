using VeriWork_Admin.Application.Models;
using VeriWork_Admin.Core.Entities;

namespace VeriWork_Admin.Application.Mappers;

public static class RegistrationMapper
{
    public static Employee ToEmployee(this RegistrationModel model, string photoUrl, string hashedPassword)
    {
        return new Employee
        {
            EmployeeId = Guid.NewGuid().ToString(),
            IdNumber = model.IdNumber,
            Name = model.Name,
            Surname = model.Surname,
            Phone = model.Phone,
            Address = model.Address,
            Role = "Employee",          // force role for registration
            Email = model.Email,
            PasswordHash = hashedPassword, // store hashed password, not plain
            DepartmentId = null,          // could be assigned later
            PhotoUrl = photoUrl           // link returned from Firebase
        };
    }
}