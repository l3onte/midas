using System.Runtime.Serialization;

namespace midasMVC.Models;

public enum AccountType
{
    [EnumMember(Value = "efectivo")]
    Efectivo,

    [EnumMember(Value = "banco")]
    Banco,

    [EnumMember(Value = "tarjeta_credito")]
    TarjetaCredito
}

public class Account
{
    public int Id { get; set; }
    public int User_id { get; set; }
    public string Name { get; set; } = String.Empty;
    public decimal Balance { get; set; }
    public AccountType Account_type { get; set; }
    public bool Status { get; set; }
    public DateTime Created_at { get; set; } = DateTime.UtcNow;
    public DateTime Updated_at { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}