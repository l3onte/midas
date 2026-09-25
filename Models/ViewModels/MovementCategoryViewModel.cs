namespace midasMVC.Models.ViewModels;

public class MovementCategoryViewModel
{
    public int Id { get; set; }
    public int User_id { get; set; }
    public string Name { get; set; } = string.Empty;

    public decimal? BudgetAmount { get; set; }
}