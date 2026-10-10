using System.ComponentModel.DataAnnotations;

namespace IAjudinhaApi.DTOs;

public class ChamadoCreateDto
{
    [Required(ErrorMessage = "Informe o assunto.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "O assunto deve ter entre 5 e 200 caracteres.")]
    public string Assunto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descreva o problema.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 2000 caracteres.")]
    public string Descricao { get; set; } = string.Empty;
}
