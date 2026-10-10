using System.Security.Claims;
using IAjudinhaApi.Models;
using IAjudinhaApi.Seguranca;

namespace IAjudinhaApi.Data;

public static class ChamadoQueryExtensions
{
    // Funcionários enxergam todos os chamados; clientes, apenas os próprios.
    public static IQueryable<Chamado> ChamadosVisiveisPara(this IAjudinhaDbContext context, ClaimsPrincipal usuario)
    {
        if (usuario.IsInRole(Perfis.Funcionario))
            return context.Chamados;

        var idCliente = usuario.ObterId();
        return context.Chamados.Where(c => c.IdCliente == idCliente);
    }
}
