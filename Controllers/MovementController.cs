using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using midasMVC.Data;
using midasMVC.Models;

namespace midasMVC.Controllers;

[Authorize]
public class MovementController : Controller
{
    private readonly MovementRepository _movementRepository;

    public MovementController(MovementRepository movementRepository)
    {
        _movementRepository = movementRepository;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Movement movement, int? goalId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        movement.User_id = userId;
        await _movementRepository.CreateMovementAsync(movement, goalId);

        return RedirectToAction("Movimientos", "Home");
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int id) ? id : 0;
    }
}