using Microsoft.AspNetCore.Mvc;
using midasMVC.Models;

namespace MyApp.Namespace
{
    public class LoanPaymentHistoryController : Controller
    {
        private readonly LoanPaymentHistoryRepository _paymentHistoryRepository;

        public LoanPaymentHistoryController(
            LoanPaymentHistoryRepository paymentHistoryRepository)
        {
            _paymentHistoryRepository = paymentHistoryRepository;
        }

        public async Task<IActionResult> Index(int loanId)
        {
            var paymentHistory =
                await _paymentHistoryRepository
                    .GetPaymentHistoryByLoanIdAsync(loanId);

            ViewBag.LoanId = loanId;

            return View(paymentHistory);
        }
    }
}