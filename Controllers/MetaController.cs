using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using midasMVC.Models;
using MySqlConnector;

namespace MyApp.Namespace
{
    public class MetaController : Controller
    {
        private readonly MetasRepository _metasRepository;
        public MetaController(MetasRepository metasRepository)
        {
            _metasRepository = metasRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Usuario Premium, Usuario Free")]
        public async Task<IActionResult> Create(Goal goal)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                goal.User_id = userId;
            }

            if (ModelState.IsValid)
            {
                await _metasRepository.CreateGoalAsync(userId, goal);
            }

            return RedirectToAction("MyGoals", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Usuario Premium")]
        public async Task<IActionResult> Update(int goalId, Goal goal)
        {
            if (ModelState.IsValid)
            {
                await _metasRepository.UpdateGoalAsync(goalId, goal);
            }

            return RedirectToAction("MyGoals", "Home");
        }

    }
}
