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

    public User? User { get; set; }
    public MovementCategory? MovementCategory { get; set; }

}