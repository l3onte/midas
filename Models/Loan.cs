namespace midasMVC.Models;

public class Loans
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PersonName { get; set; } = String.Empty;
    public decimal Amount { get; set; }
    public LoanType Type { get; set; }
    public decimal InterestRate { get; set; }
    public string Description { get; set; } = String.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public bool Status { get; set; } = true;

    public User? User { get; set; }
}