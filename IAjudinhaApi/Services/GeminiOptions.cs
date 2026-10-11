namespace IAjudinhaApi.Services;

public class GeminiOptions
{
    public const string Secao = "Gemini";

    // Gere em https://aistudio.google.com/apikey. Nunca coloque a chave no front-end ou no Git.
    public string ApiKey { get; set; } = string.Empty;

    // O alias "latest" acompanha a versão estável mais recente, evitando quebrar quando versões antigas são desligadas.
    // O Flash-Lite responde em ~1 s e dá conta de um chat de suporte; o Flash mais novo chegou a levar mais de 30 s
    // e a responder 503 (sobrecarga) com frequência.
    public string Modelo { get; set; } = "gemini-flash-lite-latest";

    // Quanto o modelo "pensa" antes de responder (minimal, low, medium, high). Respostas curtas de chat não precisam
    // de mais que "low", que é aceito por todos os modelos 3.x. Deixe vazio para usar o padrão do modelo.
    public string NivelRaciocinio { get; set; } = "low";

    // Permite que a IA consulte o Google para responder sobre assuntos atuais (consome cota extra).
    public bool UsarBuscaGoogle { get; set; }
}
