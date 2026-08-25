using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using midasMVC.Data;
using midasMVC.Models;

namespace midasMVC.Controllers;

[Authorize]
public class MovementCategoryController : Controller
{
    private readonly MovementCategoryRepository _movementCategoryRepository;

    public MovementCategoryController(MovementCategoryRepository movementCategoryRepository)
    {
        _movementCategoryRepository = movementCategoryRepository;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario Premium")]
    public async Task<IActionResult> Create(MovementCategory movementCategory)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            movementCategory.User_id = userId;
        }

        if (ModelState.IsValid)
        {
            await _movementCategoryRepository.CreateMovementCategoryAsync(movementCategory);
        }

        return RedirectToAction("MovementCategories", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario Premium")]
    public async Task<IActionResult> Edit(int categorieId, MovementCategory movementCategory)
    {
        if (ModelState.IsValid)
        {
            await _movementCategoryRepository.UpdateMovementCategoryAsync(categorieId, movementCategory);
        }

        return RedirectToAction("MovementCategories", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario Premium")]
    public async Task<IActionResult> Delete(int categorieId)
    {
        if (ModelState.IsValid)
        {
            await _movementCategoryRepository.DeleteMovementCategoryAsync(categorieId);
        }

        return RedirectToAction("MovementCategorie", "Home");
    }

}