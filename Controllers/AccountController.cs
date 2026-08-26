using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using midasMVC.Models;

namespace MyApp.Namespace
{
    public class AccountController : Controller
    {
        private readonly CuentasRepository _cuentasRepository;

        public AccountController(CuentasRepository cuentasRepository)
        {
            _cuentasRepository = cuentasRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Usuario Premium, Usuario Free")]
        public async Task<IActionResult> Create(Account account)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdClaim, out int userId))
            {
                account.User_id = userId;
            }

            if (ModelState.IsValid)
            {
                await _cuentasRepository.CreateAccountAsync(account);
            }

            return RedirectToAction("MyAccounts", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Usuario Premium, Usuario Free")]
        public async Task<IActionResult> Edit(int accountId, Account account)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdClaim, out int userId) && ModelState.IsValid)
            {
                await _cuentasRepository.EditAccountAsync(accountId, userId, account);
            }

            return RedirectToAction("MyAccounts", "Home");
        }
    }
}
