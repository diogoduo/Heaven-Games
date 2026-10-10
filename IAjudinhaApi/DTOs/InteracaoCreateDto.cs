using System.ComponentModel.DataAnnotations;

namespace IAjudinhaApi.DTOs;

public class InteracaoCreateDto
{
    [Required(ErrorMessage = "Escreva uma mensagem.")]
    [StringLength(2000, ErrorMessage = "A mensagem deve ter no máximo 2000 caracteres.")]
    public string Mensagem { get; set; } = string.Empty;
}
