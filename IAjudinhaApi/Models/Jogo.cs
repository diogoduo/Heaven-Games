namespace IAjudinhaApi.Models;

public class Jogo
{
    public int IdJogo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    // Gêneros separados por " / ", ex.: "Ação / Aventura". Usado também nos filtros da loja.
    public string Genero { get; set; } = string.Empty;
    // Nulo quando o jogo é gratuito.
    public decimal? Preco { get; set; }
    public string Imagem { get; set; } = string.Empty;
    public bool EmDestaque { get; set; }
}
