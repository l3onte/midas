using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using midasMVC.Data;
using midasMVC.Models;

namespace MyApp.Namespace
{
    [Authorize]
    public class SuscriptionController : Controller
    {
        private readonly SubscriptionRepository _subscriptionRepository;

        public SuscriptionController(SubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult PaymentSubscription(int subscriptionId)
        {
            if (subscriptionId != 2 && subscriptionId != 3)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SubscriptionId = subscriptionId;

            return View(new PaymentMethod());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PaymentSubscription(
            PaymentMethod paymentMethod,
            int subscriptionId)
        {
            if (subscriptionId != 2 && subscriptionId != 3)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SubscriptionId = subscriptionId;

            if (!ModelState.IsValid)
            {
                return View(paymentMethod);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            paymentMethod.UserId = userId;

            try
            {
                var result = await _subscriptionRepository
                    .CreatePaymentMethodAndPaySuscriptionAsync(
                        paymentMethod,
                        subscriptionId
                    );

                if (!result)
                {
                    ModelState.AddModelError(
                        "CardNumber",
                        "Ya tienes registrada esta tarjeta."
                    );

                    return View(paymentMethod);
                }

                var claims = User.Claims
                    .Where(c => c.Type != ClaimTypes.Role)
                    .ToList();

                claims.Add(new Claim(ClaimTypes.Role, "Usuario Premium"));

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal
                );

                return RedirectToAction(nameof(Success));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    $"No se pudo procesar la suscripción: {ex.Message}"
                );

                return View(paymentMethod);
            }
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}