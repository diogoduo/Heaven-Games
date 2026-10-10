using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using IAjudinhaApi.Data;
using IAjudinhaApi.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IAjudinhaApi.Services;

// Encaminha a conversa do chat para a API do Gemini, usando o catálogo da loja como contexto.
public class GeminiChatService(
    HttpClient httpClient,
    IOptions<GeminiOptions> options,
    IAjudinhaDbContext context,
    ILogger<GeminiChatService> logger)
{
    private const int MaximoMensagens = 20;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public bool Configurado => !string.IsNullOrWhiteSpace(options.Value.ApiKey);

    public async Task<ChatResponseDto> ResponderAsync(IEnumerable<ChatMensagemDto> mensagens, CancellationToken cancellationToken)
    {
        var config = options.Value;

        // O Gemini espera que a conversa comece por uma mensagem do usuário.
        var conteudos = mensagens
            .TakeLast(MaximoMensagens)
            .SkipWhile(m => m.Papel != "usuario")
            .Select(m => new GeminiContent(m.Papel == "usuario" ? "user" : "model", [new GeminiPart(m.Texto)]))
            .ToList();

        if (conteudos.Count == 0)
            throw new ChatIndisponivelException("Envie uma pergunta para a IAjudinha.", StatusCodes.Status400BadRequest);

        var corpo = new GeminiRequest(
            conteudos,
            new GeminiContent(null, [new GeminiPart(await MontarInstrucoesAsync(cancellationToken))]),
            config.UsarBuscaGoogle ? [new GeminiTool(new { })] : null);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"v1beta/models/{config.Modelo}:generateContent")
        {
            Content = JsonContent.Create(corpo, options: JsonOptions)
        };
        request.Headers.Add("x-goog-api-key", config.ApiKey);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is (HttpRequestException or TaskCanceledException) && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Falha de comunicação com o Gemini.");
            throw new ChatIndisponivelException("Não consegui falar com a IA agora. Tente novamente em instantes.", StatusCodes.Status502BadGateway);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("Gemini respondeu {StatusCode}: {Erro}", (int)response.StatusCode, erro);

                throw response.StatusCode == HttpStatusCode.TooManyRequests
                    ? new ChatIndisponivelException("A IAjudinha recebeu muitas perguntas agora. Aguarde alguns segundos e tente de novo.", StatusCodes.Status429TooManyRequests)
                    : new ChatIndisponivelException("A IAjudinha está indisponível no momento. Tente novamente mais tarde.", StatusCodes.Status502BadGateway);
            }

            var resultado = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, cancellationToken);
            var candidato = resultado?.Candidates?.FirstOrDefault();
            var texto = string.Concat(candidato?.Content?.Parts?.Select(p => p.Text) ?? []).Trim();

            if (texto.Length == 0)
                throw new ChatIndisponivelException("Não consegui formular uma resposta. Pode reformular a pergunta?", StatusCodes.Status502BadGateway);

            var fontes = candidato?.GroundingMetadata?.GroundingChunks?
                .Where(c => c.Web?.Uri is not null)
                .Select(c => new FonteDto(c.Web!.Title ?? "Fonte", c.Web.Uri!))
                .DistinctBy(f => f.Url)
                .Take(3)
                .ToList() ?? [];

            return new ChatResponseDto(texto, fontes);
        }
    }

    private async Task<string> MontarInstrucoesAsync(CancellationToken cancellationToken)
    {
        var jogos = await context.Jogos.AsNoTracking().OrderBy(j => j.Titulo).ToListAsync(cancellationToken);
        var catalogo = string.Join('\n', jogos.Select(j => $"- {j.Titulo} ({j.Genero}): {FormatarPreco(j.Preco)}"));

        return $"""
            Você é a IAjudinha, assistente virtual da Heaven Games, uma loja online de jogos que celebra a diversidade e a inclusão.

            Como responder:
            - Sempre em português do Brasil, com no máximo 4 frases, de forma simpática e acessível.
            - Ajude com dúvidas sobre os jogos do catálogo, compras, conta e problemas técnicos simples.
            - Use apenas os jogos e preços do catálogo abaixo. Não invente promoções, políticas ou jogos; se não souber, diga que não sabe.
            - Quando o caso precisar de atendimento humano (cobrança, problemas na conta, erro que persiste), oriente a pessoa a digitar "abrir chamado" aqui no chat.

            Catálogo da loja:
            {catalogo}
            """;
    }

    private static string FormatarPreco(decimal? preco) =>
        preco is null ? "gratuito" : "R$ " + preco.Value.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');

    private record GeminiRequest(List<GeminiContent> Contents, GeminiContent SystemInstruction, List<GeminiTool>? Tools);
    private record GeminiContent(string? Role, List<GeminiPart>? Parts);
    private record GeminiPart(string? Text);
    private record GeminiTool(object GoogleSearch);
    private record GeminiResponse(List<GeminiCandidate>? Candidates);
    private record GeminiCandidate(GeminiContent? Content, GeminiGroundingMetadata? GroundingMetadata);
    private record GeminiGroundingMetadata(List<GeminiGroundingChunk>? GroundingChunks);
    private record GeminiGroundingChunk(GeminiWeb? Web);
    private record GeminiWeb(string? Uri, string? Title);
}
