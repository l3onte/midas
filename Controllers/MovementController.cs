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
    public async Task<IActionResult> Create(Movement movement)
    {
        int userId = GetCurrentUserId();

        if (userId > 0 && movement.Account_id > 0 && movement.Movement_categorie_id > 0 && movement.Movement_type_id > 0)
        {
            movement.User_id = userId;
            await _movementRepository.CreateMovementAsync(movement);
        }

        return RedirectToAction("Movimientos", "Home");
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int id) ? id : 0;
    }
}