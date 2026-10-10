using System.ComponentModel.DataAnnotations;

namespace IAjudinhaApi.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [MaxLength(100)]
    public string Senha { get; set; } = string.Empty;
}
