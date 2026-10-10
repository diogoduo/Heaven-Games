using System.ComponentModel.DataAnnotations;

namespace IAjudinhaApi.DTOs;

public class ClienteRegisterDto
{
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    public DateOnly? DataNascimento { get; set; }

    // Aceita com ou sem máscara: "(11) 99999-9999" ou "11999999999".
    [Required(ErrorMessage = "Informe o telefone.")]
    [RegularExpression(@"^\D*(\d\D*){10,11}$", ErrorMessage = "Informe um telefone com DDD (10 ou 11 dígitos).")]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [MaxLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe uma senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    public string Senha { get; set; } = string.Empty;
}
