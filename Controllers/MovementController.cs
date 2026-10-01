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
    private readonly LoanRepository _loanRepository;

    public MovementController(
        MovementRepository movementRepository,
        LoanRepository loanRepository
    )
    {
        _movementRepository = movementRepository;
        _loanRepository = loanRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetLoans()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (
            string.IsNullOrEmpty(userIdClaim) ||
            !int.TryParse(userIdClaim, out int userId)
        )
        {
            return Unauthorized();
        }

        var loans =
            await _loanRepository.GetLoansForPaymentAsync(userId);

        return Json(loans);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Movement movement,
        int? goalId,
        int? loanId
    )
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) ||
            !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        movement.User_id = userId;

        await _movementRepository.CreateMovementAsync(
            movement,
            goalId,
            loanId
        );

        return RedirectToAction("Movimientos", "Home");
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int id) ? id : 0;
    }
}