using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using midasMVC.Data;
using midasMVC.Models.ViewModels;

namespace MyApp.Namespace
{
    public class CuentaController : Controller
    {
        private readonly UserRepository _useRepository;

        public CuentaController(UserRepository userRepository)
        {
            _useRepository = userRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");

            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _useRepository.GetByEmailAsync(model.Email);

            if (user is null || !user.Status)
            {
                ModelState.AddModelError("", "Correo o contrasenia incorrectos.");
                return View(model);
            }

            bool passwordOk;
            try
            {
                passwordOk = BCrypt.Net.BCrypt.Verify(
                    model.Password,
                    user.Password
                );
            }
            catch
            {
                passwordOk = false;
            }

            if (!passwordOk)
            {
                ModelState.AddModelError("", "Correo o contrasenia incorrectos.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role_id.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, principal, properties
            );

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Ping()
        {
            return NoContent();
        }

        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}
