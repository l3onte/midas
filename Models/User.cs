namespace midasMVC.Models;

public class User()
{
    public int Id { get; set; }
    public int Role_id { get; set; } = 1;
    public string Name { get; set; } = String.Empty;
    public string Last_name { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;
    public string Phone { get; set; } = String.Empty;
    public bool Status { get; set; }
    public DateTime Created_at { get; set; } = DateTime.UtcNow;
    public DateTime Updated_at { get; set; } = DateTime.UtcNow;
}