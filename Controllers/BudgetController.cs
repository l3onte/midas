using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using midasMVC.Data;
using midasMVC.Models;
using System.Security.Claims;

namespace MyApp.Namespace
{
    public class BudgetController : Controller
    {
        private readonly BudgetRepository _budgetRepository;
        private readonly MovementCategoryRepository _movementCategoryRepository;

        public BudgetController(BudgetRepository budgetRepository, MovementCategoryRepository movementCategoryRepository)
        {
            _budgetRepository = budgetRepository;
            _movementCategoryRepository = movementCategoryRepository;
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
            var categories = await _movementCategoryRepository.GetMovementCategoriesByUserIdAsync(userId);

            ViewBag.Categories = categories;

            return View(budgets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Usuario Premium")]
        public async Task<IActionResult> Create(Budget budget)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            budget.UserId = userId;
            budget.Status = true;

            if (ModelState.IsValid)
            {
                await _budgetRepository.CreateBudgetAsync(budget);
            }

            return RedirectToAction("Index", "Budget");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Usuario Premium")]
        public async Task<IActionResult> Update(int budgetId, Budget budget)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            budget.UserId = userId;
            budget.Status = true;

            if (ModelState.IsValid)
            {
                await _budgetRepository.UpdateBudgetAsync(budgetId, userId, budget);
            }

            return RedirectToAction("Index", "Budget");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Usuario Premium")]
        public async Task<IActionResult> Delete(int budgetId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            await _budgetRepository.DeleteBudgetAsync(budgetId, userId);

            return RedirectToAction("Index", "Budget");
        }
    }
}
