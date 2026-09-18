namespace midasMVC.Models;

public class Suscription
{
    public int Id { get; set; }
    public int PaymentMethodId { get; set; }
    public int UserId { get; set; }
    public int PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool Status { get; set; } = true;

    public PaymentMethod? PaymentMethod { get; set; }
    public User? User { get; set; }
    public SubscriptionPlan? Plan { get; set; }

}