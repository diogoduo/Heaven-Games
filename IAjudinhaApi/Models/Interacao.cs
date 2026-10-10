namespace IAjudinhaApi.Models;

// Mensagem trocada entre cliente e equipe dentro de um chamado.
public class Interacao
{
    public int IdInteracao { get; set; }
    public int IdChamado { get; set; }
    public Chamado? Chamado { get; set; }
    public TipoAutor TipoAutor { get; set; }
    public string NomeAutor { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}
