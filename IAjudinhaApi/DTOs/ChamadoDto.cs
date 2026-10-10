using IAjudinhaApi.Models;

namespace IAjudinhaApi.DTOs;

public record ChamadoResumoDto(
    int IdChamado,
    string Assunto,
    StatusChamado Status,
    DateTime DataInicio,
    DateTime DataAtualizacao,
    DateTime? DataFim,
    string NomeCliente,
    string EmailCliente,
    string? NomeResponsavel);

public record ChamadoDetalheDto(
    int IdChamado,
    string Assunto,
    string Descricao,
    StatusChamado Status,
    DateTime DataInicio,
    DateTime DataAtualizacao,
    DateTime? DataFim,
    string NomeCliente,
    string EmailCliente,
    string? NomeResponsavel,
    List<InteracaoDto> Interacoes);
