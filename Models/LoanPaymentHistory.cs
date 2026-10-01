namespace midasMVC.Models;

public class LoanPaymentHistory
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }

    public Loans? Loan { get; set; }
}