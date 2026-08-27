namespace midasMVC.Models;

public class Goal
{
    public int Id { get; set; }
    public int User_id { get; set; }
    public string Name { get; set; } = String.Empty;
    public decimal Target_amount { get; set; }
    public decimal Current_amount { get; set; }
    public bool Status { get; set; } = true;

    public User? User { get; set; }
}