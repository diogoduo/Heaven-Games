namespace IAjudinhaApi.Models;

public class Funcionario
{
    public int IdFuncionario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string EmailCorporativo { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public DateOnly DataNascimento { get; set; }
    public string SenhaHash { get; set; } = string.Empty;
}
