namespace IAjudinhaApi.DTOs;

public record JogoDto(int IdJogo, string Titulo, string Genero, decimal? Preco, string Imagem, bool EmDestaque);
