using IAjudinhaApi.Data;
using IAjudinhaApi.DTOs;
using IAjudinhaApi.Models;
using IAjudinhaApi.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAjudinhaApi.Controllers;

// Gestão da equipe de suporte. Apenas funcionários autenticados têm acesso.
[ApiController]
[Route("api/funcionarios")]
[Authorize(Roles = Perfis.Funcionario)]
public class FuncionariosController(IAjudinhaDbContext context) : ControllerBase
{
    // GET: api/funcionarios
    [HttpGet]
    public async Task<ActionResult<List<FuncionarioResponseDto>>> GetAll() =>
        await context.Funcionarios
            .AsNoTracking()
            .OrderBy(f => f.Nome)
            .Select(f => new FuncionarioResponseDto(f.IdFuncionario, f.Nome, f.EmailCorporativo, f.DataNascimento, f.Telefone))
            .ToListAsync();

    // GET: api/funcionarios/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FuncionarioResponseDto>> GetById(int id)
    {
        var funcionario = await context.Funcionarios.FindAsync(id);
        if (funcionario is null)
            return NotFound();

        return ParaResposta(funcionario);
    }

    // POST: api/funcionarios
    [HttpPost]
    [ProducesResponseType<FuncionarioResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(FuncionarioDto dto)
    {
        var email = dto.EmailCorporativo.Trim().ToLowerInvariant();
        if (await context.Funcionarios.AnyAsync(f => f.EmailCorporativo == email))
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: "Este e-mail corporativo já está em uso.");

        var funcionario = new Funcionario
        {
            Nome = dto.Nome.Trim(),
            EmailCorporativo = email,
            Telefone = SomenteDigitos(dto.Telefone),
            DataNascimento = dto.DataNascimento!.Value,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
        };

        context.Funcionarios.Add(funcionario);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = funcionario.IdFuncionario }, ParaResposta(funcionario));
    }

    // PUT: api/funcionarios/{id}
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, FuncionarioUpdateDto dto)
    {
        var funcionario = await context.Funcionarios.FindAsync(id);
        if (funcionario is null)
            return NotFound();

        funcionario.Nome = dto.Nome.Trim();
        funcionario.Telefone = SomenteDigitos(dto.Telefone);
        funcionario.DataNascimento = dto.DataNascimento!.Value;

        if (!string.IsNullOrEmpty(dto.Senha))
            funcionario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

        await context.SaveChangesAsync();
        return NoContent();
    }

    private static FuncionarioResponseDto ParaResposta(Funcionario f) =>
        new(f.IdFuncionario, f.Nome, f.EmailCorporativo, f.DataNascimento, f.Telefone);

    private static string? SomenteDigitos(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : new string(valor.Where(char.IsDigit).ToArray());
}
