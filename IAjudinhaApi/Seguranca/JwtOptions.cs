namespace IAjudinhaApi.Seguranca;

public class JwtOptions
{
    public const string Secao = "Jwt";

    // Chave de assinatura HMAC; precisa ter pelo menos 32 caracteres.
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "IAjudinhaApi";
    public string Audience { get; set; } = "HeavenGames";
    public int ExpiracaoHoras { get; set; } = 8;
}
