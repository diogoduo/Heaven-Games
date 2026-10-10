namespace IAjudinhaApi.Models;

public class Chamado
{
    public int IdChamado { get; set; }
    public string Assunto { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public StatusChamado Status { get; set; } = StatusChamado.Aberto;
    public DateTime DataInicio { get; set; } = DateTime.UtcNow;
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataFim { get; set; }

    public int IdCliente { get; set; }
    public Cliente? Cliente { get; set; }

    public int? IdFuncionarioResponsavel { get; set; }
    public Funcionario? FuncionarioResponsavel { get; set; }

    public List<Interacao> Interacoes { get; set; } = [];
}
