using Microsoft.AspNetCore.Mvc;

namespace VeriWork_Admin.Controllers;

public class RegisterController : Controller
{
    private readonly ILogger<RegisterController> _logger;

    public RegisterController(ILogger<RegisterController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
}