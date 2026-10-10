using System.ComponentModel.DataAnnotations;

namespace IAjudinhaApi.DTOs;

public class ChatRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Envie ao menos uma mensagem.")]
    [MaxLength(20, ErrorMessage = "Envie no máximo 20 mensagens de histórico.")]
    public List<ChatMensagemDto> Mensagens { get; set; } = [];
}

public class ChatMensagemDto
{
    // "usuario" ou "assistente"
    [Required]
    [RegularExpression("^(usuario|assistente)$", ErrorMessage = "Papel deve ser 'usuario' ou 'assistente'.")]
    public string Papel { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, ErrorMessage = "Cada mensagem pode ter no máximo 1000 caracteres.")]
    public string Texto { get; set; } = string.Empty;
}

public record FonteDto(string Titulo, string Url);

public record ChatResponseDto(string Resposta, List<FonteDto> Fontes);
