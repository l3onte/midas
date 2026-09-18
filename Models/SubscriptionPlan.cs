namespace midasMVC.Models;

public class SubscriptionPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public bool Status { get; set; } = true;
}