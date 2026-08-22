using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace midasMVC.Models.ViewModels;

public class MovementCreateViewModel
{
    [Required(ErrorMessage = "Seleccione una cuenta")]
    public int Account_id { get; set; }

    [Required(ErrorMessage = "Seleccione una categoría")]
    public int Movement_categorie_id { get; set; }

    [Required(ErrorMessage = "Seleccione el tipo de movimiento")]
    public int Movement_type_id { get; set; }

    [Required(ErrorMessage = "Ingrese una descripción")]
    [StringLength(255)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese un monto válido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Amount { get; set; }

    // Listas para poblar los Selects en la vista
    public List<SelectListItem> Accounts { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Types { get; set; } = new();
}