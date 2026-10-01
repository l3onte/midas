namespace midasMVC.Models;

public class LoanPaymentOption
{
    public int Id { get; set; }

    public string PersonName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public LoanType Type { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount { get; set; }
}