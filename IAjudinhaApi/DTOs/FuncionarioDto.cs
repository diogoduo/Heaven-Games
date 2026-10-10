using System.ComponentModel.DataAnnotations;

namespace IAjudinhaApi.DTOs;

public class FuncionarioDto
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    public DateOnly? DataNascimento { get; set; }

    [RegularExpression(@"^\D*(\d\D*){10,11}$", ErrorMessage = "Informe um telefone com DDD (10 ou 11 dígitos).")]
    public string? Telefone { get; set; }

    [Required(ErrorMessage = "Informe o e-mail corporativo.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [MaxLength(100)]
    public string EmailCorporativo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe uma senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    public string Senha { get; set; } = string.Empty;
}
