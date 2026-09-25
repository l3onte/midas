namespace midasMVC.Models;

public class Budget
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public bool Status { get; set; } = true;

    public decimal Spent { get; set; }

    public decimal Available => Amount - Spent;

    public decimal Percentage
    {
        get
        {
            if (Amount <= 0)
                return 0;

            return (Spent / Amount) * 100;
        }
    }

    public User? User { get; set; }
    public MovementCategory? MovementCategory { get; set; }
}