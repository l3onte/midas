namespace midasMVC.Models.ViewModels;

public class DashboardUserViewModel
{
    public decimal TotalGastado { get; set; }
    public decimal TotalIngresado { get; set; }
    public string CategoriaMasGastada { get; set; } = "N/A";
    public decimal MontoCategoriaMasGastada { get; set; }

    public List<string> CategoriasNombres { get; set; } = new();
    public List<decimal> CategoriasGastos { get; set; } = new();
    public List<string> MesesNombres { get; set; } = new();
    public List<decimal> MensualGastos { get; set; } = new();
    public List<decimal> MensualIngresos { get; set; } = new();
}