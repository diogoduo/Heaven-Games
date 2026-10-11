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

    // O Gemini às vezes responde 503 ("model is overloaded") por alguns segundos; vale uma segunda tentativa.
    private const int MaximoTentativas = 2;

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
            config.UsarBuscaGoogle ? [new GeminiTool(new { })] : null,
            string.IsNullOrWhiteSpace(config.NivelRaciocinio)
                ? null
                : new GeminiGenerationConfig(new GeminiThinkingConfig(config.NivelRaciocinio)));

        HttpResponseMessage response;
        try
        {
            response = await EnviarAsync(corpo, config, cancellationToken);
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

            logger.LogInformation("Gemini respondeu com o modelo {Modelo} ({Motivo}, {TokensRaciocinio} tokens de raciocínio).",
                resultado?.ModelVersion, candidato?.FinishReason, resultado?.UsageMetadata?.ThoughtsTokenCount ?? 0);

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

    private async Task<HttpResponseMessage> EnviarAsync(GeminiRequest corpo, GeminiOptions config, CancellationToken cancellationToken)
    {
        for (var tentativa = 1; ; tentativa++)
        {
            // Uma HttpRequestMessage não pode ser reenviada, então cada tentativa monta a sua.
            using var request = new HttpRequestMessage(HttpMethod.Post, $"v1beta/models/{config.Modelo}:generateContent")
            {
                Content = JsonContent.Create(corpo, options: JsonOptions)
            };
            request.Headers.Add("x-goog-api-key", config.ApiKey);

            var response = await httpClient.SendAsync(request, cancellationToken);
            if (tentativa >= MaximoTentativas || response.StatusCode is not (HttpStatusCode.ServiceUnavailable or HttpStatusCode.InternalServerError))
                return response;

            logger.LogInformation("Gemini respondeu {StatusCode}; tentando de novo.", (int)response.StatusCode);
            response.Dispose();
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }

    private async Task<string> MontarInstrucoesAsync(CancellationToken cancellationToken)
    {
        var jogos = await context.Jogos.AsNoTracking().OrderBy(j => j.Titulo).ToListAsync(cancellationToken);
        var catalogo = string.Join('\n', jogos.Select(j => $"- {j.Titulo} ({j.Genero}): {FormatarPreco(j.Preco)}"));

        return $"""
            Você é a IAjudinha, assistente virtual da Heaven Games, uma loja online de jogos que celebra a diversidade e a inclusão.

            Como responder:
            - Sempre em português do Brasil, de forma simpática, acessível e direta: no máximo 3 frases curtas.
            - Vá direto ao ponto: nada de frases genéricas de abertura ("Com certeza!", "Ótima pergunta!") ou de encerramento.
            - Para indicar vários jogos, use uma lista curta com "- " no início de cada item e o preço ao lado. Destaque nomes de jogos com **negrito**.
            - Ajude com dúvidas sobre os jogos do catálogo, compras, conta e problemas técnicos simples.
            - Use apenas os jogos e preços do catálogo abaixo. Não invente promoções, políticas ou jogos; se não souber, diga que não sabe.
            - Quando o caso precisar de atendimento humano (cobrança, problemas na conta, código de ativação, erro que persiste), diga que a pessoa pode digitar "abrir chamado" aqui no chat.

            Catálogo da loja:
            {catalogo}
            """;
    }

    private static string FormatarPreco(decimal? preco) =>
        preco is null ? "gratuito" : "R$ " + preco.Value.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');

    private record GeminiRequest(List<GeminiContent> Contents, GeminiContent SystemInstruction, List<GeminiTool>? Tools, GeminiGenerationConfig? GenerationConfig);
    private record GeminiContent(string? Role, List<GeminiPart>? Parts);
    private record GeminiPart(string? Text);
    private record GeminiTool(object GoogleSearch);
    private record GeminiGenerationConfig(GeminiThinkingConfig ThinkingConfig);
    private record GeminiThinkingConfig(string ThinkingLevel);
    private record GeminiResponse(List<GeminiCandidate>? Candidates, string? ModelVersion, GeminiUsageMetadata? UsageMetadata);
    private record GeminiUsageMetadata(int? ThoughtsTokenCount);
    private record GeminiCandidate(GeminiContent? Content, string? FinishReason, GeminiGroundingMetadata? GroundingMetadata);
    private record GeminiGroundingMetadata(List<GeminiGroundingChunk>? GroundingChunks);
    private record GeminiGroundingChunk(GeminiWeb? Web);
    private record GeminiWeb(string? Uri, string? Title);
}
