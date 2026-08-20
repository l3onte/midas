using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using midasMVC.Models;

namespace midasMVC.Controllers;

public class HomeController : Controller
{
    [Authorize]
    public IActionResult Index()
    {
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

    [Authorize(Roles = "Administrador")]
    public IActionResult Administrador()
    {
        return View();
    }

    [Authorize(Roles = "Usuario Free")]
    public IActionResult Free_Version()
    {
        return View();
    }

    [Authorize(Roles = "Usuario Premium")]
    public IActionResult Premium_Version()
    {
        return View();
    }
}
