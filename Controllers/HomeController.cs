using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using midasMVC.Data;
using midasMVC.Models;

namespace midasMVC.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly UserRepository _userRepository;
    private readonly MovementRepository _movementRepository;

    public HomeController(UserRepository userRepository, MovementRepository movementRepository)
    {
        _userRepository = userRepository;
        _movementRepository = movementRepository;
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Inicio_Administrador()
    {
        var stats = await _userRepository.GetUsersStatsAsync();
        return View(stats);
    }

    [Authorize(Roles = "Usuario Free")]
    public async Task<IActionResult> Inicio_User_Free()
    {
        return View();
    }


    [Authorize]
    public IActionResult Index()
    {
        if (User.IsInRole("Administrador"))
        {
            return RedirectToAction("Inicio_Administrador");
        }

        if (User.IsInRole("Usuario Free"))
        {
            return RedirectToAction("Inicio_User_Free");
        }

            return View();
    }

    public IActionResult Privacy() => View();

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Administracion()
    {
        var users = await _userRepository.GetUsersAsync();
        return View(users);
    }

    [Authorize(Roles = "Usuario Free, Usuario Premium")]
    public async Task<IActionResult> Movimientos()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var movements = await _movementRepository.GetMovementsByUserIdAsync(userId);

        var accounts = await _movementRepository.GetAccountsByUserIdAsync(userId);
        var categories = await _movementRepository.GetCategoriesAsync();
        var types = await _movementRepository.GetMovementTypesAsync();

        ViewBag.Accounts = accounts.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name });
        ViewBag.Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
        ViewBag.Types = types.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name });

        return View(movements);
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