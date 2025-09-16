namespace VeriWork_Admin.Core.Entities;

public class Employee : User
{
    public string EmployeeId;
    public string DepartmentId { get; set; }   
    public string PhotoUrl { get; set; } 
    
}