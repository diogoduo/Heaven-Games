using System.Linq.Expressions;
using IAjudinhaApi.Data;
using IAjudinhaApi.DTOs;
using IAjudinhaApi.Models;
using IAjudinhaApi.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAjudinhaApi.Controllers;

[ApiController]
[Route("api/chamados")]
[Authorize]
public class ChamadosController(IAjudinhaDbContext context) : ControllerBase
{
    private static readonly Expression<Func<Chamado, ChamadoResumoDto>> ParaResumo = c => new ChamadoResumoDto(
        c.IdChamado,
        c.Assunto,
        c.Status,
        c.DataInicio,
        c.DataAtualizacao,
        c.DataFim,
        c.Cliente!.Nome,
        c.Cliente.Email,
        c.FuncionarioResponsavel != null ? c.FuncionarioResponsavel.Nome : null);

    // GET: api/chamados?status=Aberto -> fila de atendimento da equipe
    [HttpGet]
    [Authorize(Roles = Perfis.Funcionario)]
    public async Task<ActionResult<List<ChamadoResumoDto>>> GetAll([FromQuery] StatusChamado? status)
    {
        var query = context.Chamados.AsNoTracking();

        if (status is not null)
            query = query.Where(c => c.Status == status);

        return await query
            .OrderByDescending(c => c.DataAtualizacao)
            .Select(ParaResumo)
            .ToListAsync();
    }

    // GET: api/chamados/meus -> chamados do cliente logado
    [HttpGet("meus")]
    [Authorize(Roles = Perfis.Cliente)]
    public async Task<ActionResult<List<ChamadoResumoDto>>> GetMeus()
    {
        var idCliente = User.ObterId();

        return await context.Chamados
            .AsNoTracking()
            .Where(c => c.IdCliente == idCliente)
            .OrderByDescending(c => c.DataAtualizacao)
            .Select(ParaResumo)
            .ToListAsync();
    }

    // GET: api/chamados/{id} -> detalhes com o histórico de mensagens
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChamadoDetalheDto>> GetById(int id)
    {
        var chamado = await BuscarDetalheAsync(id);
        return chamado is null ? NotFound() : chamado;
    }

    // POST: api/chamados -> cliente abre um novo chamado
    [HttpPost]
    [Authorize(Roles = Perfis.Cliente)]
    [ProducesResponseType<ChamadoDetalheDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(ChamadoCreateDto dto)
    {
        var agora = DateTime.UtcNow;
        var chamado = new Chamado
        {
            Assunto = dto.Assunto.Trim(),
            Descricao = dto.Descricao.Trim(),
            IdCliente = User.ObterId(),
            DataInicio = agora,
            DataAtualizacao = agora
        };

        context.Chamados.Add(chamado);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = chamado.IdChamado }, await BuscarDetalheAsync(chamado.IdChamado));
    }

    // PATCH: api/chamados/{id}/status -> equipe altera o status
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = Perfis.Funcionario)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarStatus(int id, StatusUpdateDto dto)
    {
        var chamado = await context.Chamados.FindAsync(id);
        if (chamado is null)
            return NotFound();

        var novoStatus = dto.Status!.Value;
        if (chamado.Status != novoStatus)
        {
            var agora = DateTime.UtcNow;
            chamado.Status = novoStatus;
            chamado.DataFim = novoStatus == StatusChamado.Resolvido ? agora : null;
            chamado.DataAtualizacao = agora;
            chamado.IdFuncionarioResponsavel ??= User.ObterId();
            await context.SaveChangesAsync();
        }

        return NoContent();
    }

    private Task<ChamadoDetalheDto?> BuscarDetalheAsync(int id) =>
        context.ChamadosVisiveisPara(User)
            .AsNoTracking()
            .Where(c => c.IdChamado == id)
            .Select(c => new ChamadoDetalheDto(
                c.IdChamado,
                c.Assunto,
                c.Descricao,
                c.Status,
                c.DataInicio,
                c.DataAtualizacao,
                c.DataFim,
                c.Cliente!.Nome,
                c.Cliente.Email,
                c.FuncionarioResponsavel != null ? c.FuncionarioResponsavel.Nome : null,
                c.Interacoes
                    .OrderBy(i => i.DataHora)
                    .Select(i => new InteracaoDto(i.IdInteracao, i.TipoAutor, i.NomeAutor, i.Mensagem, i.DataHora))
                    .ToList()))
            .FirstOrDefaultAsync();
}
