using IAjudinhaApi.Models;

namespace IAjudinhaApi.DTOs;

public record InteracaoDto(int IdInteracao, TipoAutor TipoAutor, string NomeAutor, string Mensagem, DateTime DataHora);
