using Microsoft.AspNetCore.Mvc;

namespace VeriWork_Admin.Controllers;

public class AuditController : Controller
{
    private readonly ILogger<AuditController> _logger;

    public AuditController(ILogger<AuditController> logger)
    {
        _logger = logger;
    }


    public IActionResult Index()
    {
        return View();
    }
}