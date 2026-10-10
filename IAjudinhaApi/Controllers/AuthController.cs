using IAjudinhaApi.Data;
using IAjudinhaApi.DTOs;
using IAjudinhaApi.Models;
using IAjudinhaApi.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IAjudinhaApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAjudinhaDbContext context, TokenService tokenService) : ControllerBase
{
    // Usado quando o e-mail não existe, para que o tempo de resposta não revele quais contas estão cadastradas.
    private static readonly string HashFicticio = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

    // POST: api/auth/cadastro -> cria a conta do cliente e já devolve o token de acesso
    [HttpPost("cadastro")]
    [EnableRateLimiting(PoliticasRateLimit.Autenticacao)]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cadastrar(ClienteRegisterDto dto)
    {
        var dataNascimento = dto.DataNascimento!.Value;
        if (dataNascimento > DateOnly.FromDateTime(DateTime.Today) || dataNascimento.Year < 1900)
        {
            ModelState.AddModelError(nameof(dto.DataNascimento), "Data de nascimento inválida.");
            return ValidationProblem(ModelState);
        }

        var email = NormalizarEmail(dto.Email);
        if (await context.Clientes.AnyAsync(c => c.Email == email))
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: "Este e-mail já está cadastrado. Tente fazer login.");

        var cliente = new Cliente
        {
            Nome = dto.Nome.Trim(),
            Email = email,
            Telefone = SomenteDigitos(dto.Telefone),
            DataNascimento = dataNascimento,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var resposta = tokenService.Gerar(cliente.IdCliente, cliente.Nome, cliente.Email, Perfis.Cliente);
        return StatusCode(StatusCodes.Status201Created, resposta);
    }

    // POST: api/auth/login -> login de cliente
    [HttpPost("login")]
    [EnableRateLimiting(PoliticasRateLimit.Autenticacao)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var email = NormalizarEmail(dto.Email);
        var cliente = await context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Email == email);

        if (!SenhaConfere(dto.Senha, cliente?.SenhaHash))
            return CredenciaisInvalidas();

        return tokenService.Gerar(cliente!.IdCliente, cliente.Nome, cliente.Email, Perfis.Cliente);
    }

    // POST: api/auth/funcionarios/login -> login da equipe com o e-mail corporativo
    [HttpPost("funcionarios/login")]
    [EnableRateLimiting(PoliticasRateLimit.Autenticacao)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> LoginFuncionario(LoginDto dto)
    {
        var email = NormalizarEmail(dto.Email);
        var funcionario = await context.Funcionarios.AsNoTracking().FirstOrDefaultAsync(f => f.EmailCorporativo == email);

        if (!SenhaConfere(dto.Senha, funcionario?.SenhaHash))
            return CredenciaisInvalidas();

        return tokenService.Gerar(funcionario!.IdFuncionario, funcionario.Nome, funcionario.EmailCorporativo, Perfis.Funcionario);
    }

    // GET: api/auth/me -> dados do usuário dono do token
    [Authorize]
    [HttpGet("me")]
    public ActionResult<UsuarioDto> Me() => new UsuarioDto(
        User.ObterId(),
        User.ObterNome(),
        User.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? string.Empty,
        User.IsInRole(Perfis.Funcionario) ? Perfis.Funcionario : Perfis.Cliente);

    private static bool SenhaConfere(string senha, string? hash)
    {
        var confere = BCrypt.Net.BCrypt.Verify(senha, hash ?? HashFicticio);
        return hash is not null && confere;
    }

    private ObjectResult CredenciaisInvalidas() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "E-mail ou senha inválidos.");

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    private static string SomenteDigitos(string valor) => new(valor.Where(char.IsDigit).ToArray());
}
