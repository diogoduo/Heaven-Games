namespace IAjudinhaApi.DTOs;

public record UsuarioDto(int Id, string Nome, string Email, string Perfil);

public record AuthResponseDto(string Token, DateTime ExpiraEm, UsuarioDto Usuario);
