using System.ComponentModel.DataAnnotations;

namespace IAjudinhaApi.DTOs;

public class FuncionarioUpdateDto
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    public DateOnly? DataNascimento { get; set; }

    [RegularExpression(@"^\D*(\d\D*){10,11}$", ErrorMessage = "Informe um telefone com DDD (10 ou 11 dígitos).")]
    public string? Telefone { get; set; }

    // Opcional: só troca a senha quando informada.
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    public string? Senha { get; set; }
}
