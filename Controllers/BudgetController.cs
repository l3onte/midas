using Microsoft.AspNetCore.Mvc;
using midasMVC.Data;
using midasMVC.Models;
using System.Security.Claims;

namespace MyApp.Namespace
{
    public class BudgetController : Controller
    {
        private readonly BudgetRepository _budgetRepository;

        public BudgetController(BudgetRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var budgets = await _budgetRepository.GetBudgetsByUserIdAsync(userId);
            return View(budgets);
        }

    }
}
