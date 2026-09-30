using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using midasMVC.Models;

namespace midasMVC.Controllers;

[Authorize]
public class LoanController : Controller
{
    private readonly LoanRepository _loanRepository;

    public LoanController(LoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Usuario Premium")]
    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var loans = await _loanRepository.GetLoansByUserIdAsync(userId);

        return View(loans);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario Premium")]
    public async Task<IActionResult> Create(Loans loans)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        if (ModelState.IsValid)
        {
            loans.UserId = userId;
            await _loanRepository.CreateLoanAsync(loans);
        }

        return RedirectToAction("Index", "Loan");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario Premium")]
    public async Task<IActionResult> Update(Loans loans)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        if (ModelState.IsValid)
        {
            loans.UserId = userId;
            await _loanRepository.UpdateLoanAsyncById(loans);
        }

        return RedirectToAction("Index", "Loan");
    }

}