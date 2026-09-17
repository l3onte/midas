namespace midasMVC.Models;

public class PaymentMethod
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string OwnerName { get; set; } = String.Empty;
    public string OwnerLastName { get; set; } = String.Empty;
    public string CardNumber { get; set; } = String.Empty;
    public string CvCode { get; set; } = String.Empty;
    public DateOnly ExpirationDate { get; set; }
    public DateTime Created_at { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}