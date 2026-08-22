using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using midasMVC.Data;
using midasMVC.Models;
using midasMVC.Models.ViewModels;

namespace MyApp.Namespace
{
    public class CuentaController : Controller
    {
        private readonly UserRepository _userRepository;

        public CuentaController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true) 
                return RedirectToAction("Index", "Home");

            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string cleanEmail = model.Email?.Trim() ?? string.Empty;
            string cleanPassword = model.Password?.Trim() ?? string.Empty;

            var user = await _userRepository.GetByEmailAsync(cleanEmail);

            if (user is null || !user.Status)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(model);
            }

            bool passwordOk = false;

            try
            {
                passwordOk = BCrypt.Net.BCrypt.Verify(cleanPassword, user.Password.Trim());
            }
            catch
            {
                passwordOk = false;
            }

            if (!passwordOk)
            {
                try
                {
                    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
                    var result = hasher.VerifyHashedPassword(user, user.Password.Trim(), cleanPassword);

                    if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success || 
                        result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        passwordOk = true;
                    }
                }
                catch
                {
                    passwordOk = false;
                }
            }

            if (!passwordOk && user.Password.Trim() == cleanPassword)
            {
                passwordOk = true;
            }

            if (!passwordOk)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(model);
            }

            if (!user.Password.Trim().StartsWith("$2"))
            {
                string newBcryptHash = BCrypt.Net.BCrypt.HashPassword(cleanPassword);
                await _userRepository.UpdatePasswordAsync(user.Id, newBcryptHash);
            }

            string roleName = user.Role?.Name ?? "Usuario Free";
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, roleName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var properties = new AuthenticationProperties { IsPersistent = model.RememberMe, AllowRefresh = true };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Ping() => NoContent();

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccesoDenegado() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Cuenta");
        }

        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Cuenta");
        }
    }
}