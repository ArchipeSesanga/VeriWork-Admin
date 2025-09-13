using Microsoft.AspNetCore.Mvc;

namespace VeriWork_Admin.Controllers;

public class RegisterEmployeeController : Controller
{
    private readonly ILogger<RegisterEmployeeController> _logger;

    public RegisterEmployeeController(ILogger<RegisterEmployeeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
}