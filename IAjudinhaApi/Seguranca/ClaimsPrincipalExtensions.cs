using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IAjudinhaApi.Seguranca;

public static class ClaimsPrincipalExtensions
{
    public static int ObterId(this ClaimsPrincipal usuario) =>
        int.Parse(usuario.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new InvalidOperationException("Token sem a claim 'sub'."));

    public static string ObterNome(this ClaimsPrincipal usuario) =>
        usuario.FindFirstValue(JwtRegisteredClaimNames.Name) ?? string.Empty;
}
