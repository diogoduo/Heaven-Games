using IAjudinhaApi.Data;
using IAjudinhaApi.DTOs;
using IAjudinhaApi.Models;
using IAjudinhaApi.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAjudinhaApi.Controllers;

// Mensagens dentro de um chamado. Clientes só acessam os próprios chamados.
[ApiController]
[Route("api/chamados/{idChamado:int}/interacoes")]
[Authorize]
public class InteracoesController(IAjudinhaDbContext context) : ControllerBase
{
    // GET: api/chamados/{idChamado}/interacoes
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<InteracaoDto>>> GetAll(int idChamado)
    {
        if (!await context.ChamadosVisiveisPara(User).AnyAsync(c => c.IdChamado == idChamado))
            return NotFound();

        return await context.Interacoes
            .AsNoTracking()
            .Where(i => i.IdChamado == idChamado)
            .OrderBy(i => i.DataHora)
            .Select(i => new InteracaoDto(i.IdInteracao, i.TipoAutor, i.NomeAutor, i.Mensagem, i.DataHora))
            .ToListAsync();
    }

    // POST: api/chamados/{idChamado}/interacoes
    [HttpPost]
    [ProducesResponseType<InteracaoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(int idChamado, InteracaoCreateDto dto)
    {
        var chamado = await context.ChamadosVisiveisPara(User).FirstOrDefaultAsync(c => c.IdChamado == idChamado);
        if (chamado is null)
            return NotFound();

        if (chamado.Status == StatusChamado.Resolvido)
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: "Este chamado já foi resolvido. Abra um novo chamado se precisar de mais ajuda.");

        var ehFuncionario = User.IsInRole(Perfis.Funcionario);
        var agora = DateTime.UtcNow;
        var interacao = new Interacao
        {
            IdChamado = idChamado,
            TipoAutor = ehFuncionario ? TipoAutor.Funcionario : TipoAutor.Cliente,
            NomeAutor = User.ObterNome(),
            Mensagem = dto.Mensagem.Trim(),
            DataHora = agora
        };
        context.Interacoes.Add(interacao);

        // A primeira resposta da equipe assume o chamado e o coloca em andamento.
        if (ehFuncionario)
        {
            chamado.IdFuncionarioResponsavel ??= User.ObterId();
            if (chamado.Status == StatusChamado.Aberto)
                chamado.Status = StatusChamado.EmAndamento;
        }
        chamado.DataAtualizacao = agora;

        await context.SaveChangesAsync();

        var resposta = new InteracaoDto(interacao.IdInteracao, interacao.TipoAutor, interacao.NomeAutor, interacao.Mensagem, interacao.DataHora);
        return CreatedAtAction(nameof(GetAll), new { idChamado }, resposta);
    }
}
