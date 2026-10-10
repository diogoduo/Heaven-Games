using System.Security.Claims;
using System.Text;
using IAjudinhaApi.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IAjudinhaApi.Seguranca;

public class TokenService(IOptions<JwtOptions> options)
{
    private readonly JsonWebTokenHandler _handler = new();

    public AuthResponseDto Gerar(int id, string nome, string email, string perfil)
    {
        var jwt = options.Value;
        var expiraEm = DateTime.UtcNow.AddHours(jwt.ExpiracaoHoras);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Expires = expiraEm,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, nome),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("role", perfil)
            ]),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        return new AuthResponseDto(_handler.CreateToken(descriptor), expiraEm, new UsuarioDto(id, nome, email, perfil));
    }
}
