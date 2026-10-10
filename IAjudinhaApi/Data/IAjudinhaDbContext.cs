using IAjudinhaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IAjudinhaApi.Data;

public class IAjudinhaDbContext(DbContextOptions<IAjudinhaDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<Chamado> Chamados => Set<Chamado>();
    public DbSet<Interacao> Interacoes => Set<Interacao>();
    public DbSet<Jogo> Jogos => Set<Jogo>();

    // Todas as datas são gravadas em UTC; ao ler do SQL Server, marca o Kind como Utc
    // para que a API serialize com "Z" e o navegador converta para o fuso local.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<DataUtcConverter>();
    }

    private class DataUtcConverter() : ValueConverter<DateTime, DateTime>(
        valor => valor.Kind == DateTimeKind.Utc ? valor : valor.ToUniversalTime(),
        valor => DateTime.SpecifyKind(valor, DateTimeKind.Utc));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(e => e.IdCliente);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Nome).HasMaxLength(150);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Telefone).HasMaxLength(11);
            entity.Property(e => e.SenhaHash).HasMaxLength(100);
        });

        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.ToTable("Funcionarios");
            entity.HasKey(e => e.IdFuncionario);
            entity.HasIndex(e => e.EmailCorporativo).IsUnique();
            entity.Property(e => e.Nome).HasMaxLength(150);
            entity.Property(e => e.EmailCorporativo).HasMaxLength(100);
            entity.Property(e => e.Telefone).HasMaxLength(11);
            entity.Property(e => e.SenhaHash).HasMaxLength(100);
        });

        modelBuilder.Entity<Chamado>(entity =>
        {
            entity.ToTable("Chamados");
            entity.HasKey(e => e.IdChamado);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.Assunto).HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Chamados)
                .HasForeignKey(e => e.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.FuncionarioResponsavel)
                .WithMany()
                .HasForeignKey(e => e.IdFuncionarioResponsavel)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Interacao>(entity =>
        {
            entity.ToTable("Interacoes");
            entity.HasKey(e => e.IdInteracao);
            entity.Property(e => e.TipoAutor).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.NomeAutor).HasMaxLength(150);
            entity.Property(e => e.Mensagem).HasMaxLength(2000);

            entity.HasOne(e => e.Chamado)
                .WithMany(c => c.Interacoes)
                .HasForeignKey(e => e.IdChamado)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Jogo>(entity =>
        {
            entity.ToTable("Jogos");
            entity.HasKey(e => e.IdJogo);
            entity.Property(e => e.Titulo).HasMaxLength(150);
            entity.Property(e => e.Genero).HasMaxLength(100);
            entity.Property(e => e.Imagem).HasMaxLength(200);
            entity.Property(e => e.Preco).HasPrecision(10, 2);
            entity.HasData(CatalogoInicial.Jogos);
        });
    }
}
