namespace IAjudinhaApi.Seguranca;

public static class PoliticasRateLimit
{
    // Login e cadastro: dificulta tentativas de adivinhar senhas.
    public const string Autenticacao = "autenticacao";

    // Chat: protege a cota da API do Gemini.
    public const string Chat = "chat";
}
