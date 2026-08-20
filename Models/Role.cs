using System.Runtime.Serialization;
using static System.DateTime;

namespace midasMVC.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public bool Status { get; set; }
    public DateTime Created_at { get; set; } = DateTime.UtcNow;
    public DateTime Updated_at { get; set; } = DateTime.UtcNow;
}