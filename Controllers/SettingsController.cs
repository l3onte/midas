using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using midasMVC.Data;
using midasMVC.Models;

namespace midasMVC.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly UserRepository _userRepository;

    public SettingsController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // GET: /Settings
    public IActionResult Index()
    {
        return View();
    }

    // GET: /Settings/Perfil
    public async Task<IActionResult> Perfil()
    {
        // Obtener el ID del usuario actualmente autenticado
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) ||
            !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        // Buscar el usuario en la base de datos
        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound("No se encontró el usuario.");
        }

        return View(user);
    }
}