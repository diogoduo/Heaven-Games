namespace IAjudinhaApi.Services;

public class GeminiOptions
{
    public const string Secao = "Gemini";

    // Gere em https://aistudio.google.com/apikey. Nunca coloque a chave no front-end ou no Git.
    public string ApiKey { get; set; } = string.Empty;

    // O alias "latest" acompanha o modelo Flash estável mais recente, evitando quebrar quando versões antigas são desligadas.
    public string Modelo { get; set; } = "gemini-flash-latest";

    // Permite que a IA consulte o Google para responder sobre assuntos atuais (consome cota extra).
    public bool UsarBuscaGoogle { get; set; }
}
