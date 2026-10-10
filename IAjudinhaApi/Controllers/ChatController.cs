using IAjudinhaApi.DTOs;
using IAjudinhaApi.Seguranca;
using IAjudinhaApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IAjudinhaApi.Controllers;

[ApiController]
[Route("api/chat")]
[EnableRateLimiting(PoliticasRateLimit.Chat)]
public class ChatController(GeminiChatService chatService) : ControllerBase
{
    // POST: api/chat -> envia o histórico da conversa e recebe a resposta da IAjudinha
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ChatResponseDto>> Enviar(ChatRequestDto dto, CancellationToken cancellationToken)
    {
        if (!chatService.Configurado)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                detail: "A IA não está configurada neste ambiente, mas você ainda pode digitar \"abrir chamado\" para falar com a nossa equipe.");
        }

        try
        {
            return await chatService.ResponderAsync(dto.Mensagens, cancellationToken);
        }
        catch (ChatIndisponivelException ex)
        {
            return Problem(statusCode: ex.StatusCode, detail: ex.Message);
        }
    }
}
