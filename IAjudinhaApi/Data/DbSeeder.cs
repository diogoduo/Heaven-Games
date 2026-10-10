using IAjudinhaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace IAjudinhaApi.Data;

// Cria contas e chamados de demonstração para quem for testar o projeto.
// As credenciais estão documentadas no README.
public static class DbSeeder
{
    public static async Task SeedAsync(IAjudinhaDbContext context)
    {
        var funcionario = await context.Funcionarios.OrderBy(f => f.IdFuncionario).FirstOrDefaultAsync();
        if (funcionario is null)
        {
            funcionario = new Funcionario
            {
                Nome = "Ana Souza",
                EmailCorporativo = "suporte@heavengames.com",
                Telefone = "11988887777",
                DataNascimento = new DateOnly(1995, 3, 12),
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Suporte@123")
            };
            context.Funcionarios.Add(funcionario);
        }

        if (!await context.Clientes.AnyAsync())
        {
            var agora = DateTime.UtcNow;
            var cliente = new Cliente
            {
                Nome = "Jogador Demo",
                Email = "jogador@exemplo.com",
                Telefone = "11999998888",
                DataNascimento = new DateOnly(2000, 7, 20),
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Jogador@123")
            };

            cliente.Chamados.Add(new Chamado
            {
                Assunto = "Green Hell não abre depois da atualização",
                Descricao = "Desde a última atualização o jogo fecha logo na tela de carregamento. Já reinstalei e o problema continua.",
                Status = StatusChamado.EmAndamento,
                DataInicio = agora.AddDays(-2),
                DataAtualizacao = agora.AddDays(-1),
                FuncionarioResponsavel = funcionario,
                Interacoes =
                [
                    new Interacao
                    {
                        TipoAutor = TipoAutor.Funcionario,
                        NomeAutor = funcionario.Nome,
                        Mensagem = "Olá! Pode verificar se os drivers da placa de vídeo estão atualizados e nos dizer o modelo dela?",
                        DataHora = agora.AddDays(-1).AddHours(-3)
                    },
                    new Interacao
                    {
                        TipoAutor = TipoAutor.Cliente,
                        NomeAutor = cliente.Nome,
                        Mensagem = "Atualizei os drivers, é uma GTX 1660. Continua fechando.",
                        DataHora = agora.AddDays(-1)
                    }
                ]
            });

            cliente.Chamados.Add(new Chamado
            {
                Assunto = "Cobrança duplicada na compra de Pacify",
                Descricao = "Comprei Pacify ontem e apareceram duas cobranças no cartão.",
                DataInicio = agora.AddHours(-5),
                DataAtualizacao = agora.AddHours(-5)
            });

            cliente.Chamados.Add(new Chamado
            {
                Assunto = "Como ativar o modo cooperativo?",
                Descricao = "Não encontrei a opção de jogar com amigos no Brawlhalla.",
                Status = StatusChamado.Resolvido,
                DataInicio = agora.AddDays(-6),
                DataAtualizacao = agora.AddDays(-5),
                DataFim = agora.AddDays(-5),
                FuncionarioResponsavel = funcionario,
                Interacoes =
                [
                    new Interacao
                    {
                        TipoAutor = TipoAutor.Funcionario,
                        NomeAutor = funcionario.Nome,
                        Mensagem = "No menu principal, entre em \"Jogo personalizado\" e convide seus amigos pela lista. Qualquer dúvida é só abrir outro chamado!",
                        DataHora = agora.AddDays(-5)
                    }
                ]
            });

            context.Clientes.Add(cliente);
        }

        await context.SaveChangesAsync();
    }
}
