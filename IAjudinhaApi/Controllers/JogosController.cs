using IAjudinhaApi.Data;
using IAjudinhaApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAjudinhaApi.Controllers;

[ApiController]
[Route("api/jogos")]
public class JogosController(IAjudinhaDbContext context) : ControllerBase
{
    // GET: api/jogos -> catálogo exibido na página inicial e na loja
    [HttpGet]
    public async Task<ActionResult<List<JogoDto>>> GetAll() =>
        await context.Jogos
            .AsNoTracking()
            .OrderBy(j => j.IdJogo)
            .Select(j => new JogoDto(j.IdJogo, j.Titulo, j.Genero, j.Preco, j.Imagem, j.EmDestaque))
            .ToListAsync();
}
