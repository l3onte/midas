using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using midasMVC.Data;
using midasMVC.Models;

namespace midasMVC.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly UserRepository _userRepository;

    public HomeController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Administracion()
    {
        var users = await _userRepository.GetUsersAsync();
        return View(users);
    }

    [Authorize(Roles = "Administrador,Usuario Free")]
    public IActionResult Free_Version() => View();

    [Authorize(Roles = "Administrador,Usuario Premium")]
    public IActionResult Premium_Version() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}