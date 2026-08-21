namespace midasMVC.Models.ViewModels;

public class UserStatsViewModel
{
    public int TotalUsuarios { get; set; }
    public int TotalActivos { get; set; }
    public int TotalInactivos { get; set; }
    public int TotalPremium { get; set; }
    public int TotalFree { get; set; }
    public int TotalAdmin { get; set; }

    public double PorcentajeFree => TotalUsuarios > 0 ? Math.Round((double)TotalFree / TotalUsuarios * 100, 2) : 0;
    public double PorcentajePremium => TotalUsuarios > 0 ? Math.Round((double)TotalPremium / TotalUsuarios * 100, 2) : 0;
    public double PorcentajeAdmin => TotalUsuarios > 0 ? Math.Round((double)TotalAdmin / TotalUsuarios * 100, 2) : 0;

}