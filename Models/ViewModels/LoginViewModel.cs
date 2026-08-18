using System.ComponentModel.DataAnnotations;
namespace midasMVC.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Escriba un correo valido.")]
    [Display(Name = "Correo Electronico.")]
    public string Email { get; set; } = String.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = String.Empty;

    [Display(Name = "Mantener Sesion Iniciada")]
    public bool RememberMe { get; set; }
}