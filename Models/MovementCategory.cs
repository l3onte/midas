namespace midasMVC.Models;

public class MovementCategory
{
    public int Id { get; set; }
    public int User_id { get; set; }
    public string Name { get; set; } = String.Empty;


    public User? User { get; set; }
}