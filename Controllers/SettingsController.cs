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

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Perfil()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) ||
            !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound("No se encontró el usuario.");
        }

        return View(user);
    }

    public async Task<IActionResult> Configuration()
    {
        return View();
    }

    [Authorize(Roles = "Usuario Free")]
    public async Task<IActionResult> GoToPremium()
    {
        return View();
    }

}