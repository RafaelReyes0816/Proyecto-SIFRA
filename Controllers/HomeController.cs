using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tienda_Repuestos_Demo.Models;

namespace Tienda_Repuestos_Demo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
        
        if (string.IsNullOrEmpty(usuarioRol))
        {
            return RedirectToAction("Login", "Account");
        }
        
        // Redirigir según el rol
        if (usuarioRol == "admin")
        {
            return RedirectToAction("Dashboard", "Admin");
        }
        else if (usuarioRol == "vendedor")
        {
            return RedirectToAction("Dashboard", "Vendedor");
        }
        else if (usuarioRol == "cliente")
        {
            return RedirectToAction("Dashboard", "Cliente");
        }
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
