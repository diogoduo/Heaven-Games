namespace IAjudinhaApi.Services;

// Falha ao obter resposta da IA. A mensagem é exibida para o usuário no chat.
public class ChatIndisponivelException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
