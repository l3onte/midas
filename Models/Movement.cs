namespace midasMVC.Models;

public class Movement
{
    public int Id { get; set; }
    public int User_id { get; set; }
    public int Account_id { get; set; }
    public int Movement_categorie_id { get; set; }
    public int Movement_type_id { get; set; }
    public string Description { get; set; } = String.Empty;
    public decimal Amount { get; set; }
    public decimal RunningBalance { get; set; }
    public DateTime Created_at { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Account? Account { get; set; }
    public MovementCategory? MovementCategory { get; set; }
    public MovementType? MovementType { get; set; }
}