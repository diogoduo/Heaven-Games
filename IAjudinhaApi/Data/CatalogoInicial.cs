using IAjudinhaApi.Models;

namespace IAjudinhaApi.Data;

// Catálogo da loja fictícia, inserido no banco pela migration.
public static class CatalogoInicial
{
    public static readonly Jogo[] Jogos =
    [
        new() { IdJogo = 1, Titulo = "Green Hell", Genero = "Aventura / Sobrevivência", Preco = 149.99m, Imagem = "img/jogos/green-hell.jpg", EmDestaque = true },
        new() { IdJogo = 2, Titulo = "Plants vs. Zombies", Genero = "Estratégia / Casual", Preco = 79.50m, Imagem = "img/jogos/plants-vs-zombies.jpg", EmDestaque = true },
        new() { IdJogo = 3, Titulo = "LazerBait", Genero = "Arcade / VR", Preco = 99.00m, Imagem = "img/jogos/lazerbait.jpg", EmDestaque = true },
        new() { IdJogo = 4, Titulo = "Slender: The Arrival", Genero = "Terror / Indie", Preco = 119.99m, Imagem = "img/jogos/slender-the-arrival.jpg", EmDestaque = true },
        new() { IdJogo = 5, Titulo = "Hordes of Hunger", Genero = "Ação / Roguelite", Preco = 89.90m, Imagem = "img/jogos/hordes-of-hunger.jpg", EmDestaque = true },
        new() { IdJogo = 6, Titulo = "Brawlhalla", Genero = "Luta / Ação", Preco = null, Imagem = "img/jogos/brawlhalla.jpg", EmDestaque = true },
        new() { IdJogo = 7, Titulo = "Pacify", Genero = "Terror / Cooperativo", Preco = 109.90m, Imagem = "img/jogos/pacify.jpg" },
        new() { IdJogo = 8, Titulo = "Supermarket Simulator", Genero = "Simulação / Indie", Preco = 65.00m, Imagem = "img/jogos/supermarket-simulator.jpg" },
        new() { IdJogo = 9, Titulo = "Steam Tactics", Genero = "Estratégia / Indie", Preco = 129.99m, Imagem = "img/jogos/steam-tactics.jpg" },
        new() { IdJogo = 10, Titulo = "Old School RPG", Genero = "RPG / Indie", Preco = 179.90m, Imagem = "img/jogos/old-school-rpg.jpg" },
        new() { IdJogo = 11, Titulo = "Red Dead Redemption 2", Genero = "Ação / Aventura", Preco = 299.00m, Imagem = "img/jogos/red-dead-redemption-2.webp" },
        new() { IdJogo = 12, Titulo = "EA SPORTS FC 26", Genero = "Esporte / Futebol", Preco = 349.99m, Imagem = "img/jogos/ea-sports-fc-26.jpg" },
        new() { IdJogo = 13, Titulo = "Call of Duty: Black Ops 7", Genero = "Ação / Tiro", Preco = 349.90m, Imagem = "img/jogos/call-of-duty-black-ops-7.jpg" },
        new() { IdJogo = 14, Titulo = "Project Motor Racing", Genero = "Corrida / Simulação", Preco = 249.90m, Imagem = "img/jogos/project-motor-racing.jpg" },
    ];
}
