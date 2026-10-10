namespace IAjudinhaApi.DTOs;

public record FuncionarioResponseDto(int IdFuncionario, string Nome, string EmailCorporativo, DateOnly DataNascimento, string? Telefone);
