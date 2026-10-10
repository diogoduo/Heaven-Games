using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IAjudinhaApi.Migrations
{
    /// <inheritdoc />
    public partial class ModeloHelpDesk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Autor",
                table: "Interacoes");

            migrationBuilder.DropColumn(
                name: "EmailUsuario",
                table: "Chamados");

            migrationBuilder.DropColumn(
                name: "StatusChamado",
                table: "Chamados");

            migrationBuilder.AlterColumn<string>(
                name: "Mensagem",
                table: "Interacoes",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(700)",
                oldMaxLength: 700);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataHora",
                table: "Interacoes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdChamado",
                table: "Interacoes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NomeAutor",
                table: "Interacoes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoAutor",
                table: "Interacoes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Telefone",
                table: "Funcionarios",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SenhaHash",
                table: "Funcionarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DataNascimento",
                table: "Funcionarios",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "SenhaHash",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DataNascimento",
                table: "Clientes",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "IdCliente",
                table: "Clientes",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCadastro",
                table: "Clientes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Chamados",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataInicio",
                table: "Chamados",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Chamados",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IdCliente",
                table: "Chamados",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdFuncionarioResponsavel",
                table: "Chamados",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Chamados",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes",
                column: "IdCliente");

            migrationBuilder.CreateTable(
                name: "Jogos",
                columns: table => new
                {
                    IdJogo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Genero = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Imagem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmDestaque = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogos", x => x.IdJogo);
                });

            migrationBuilder.InsertData(
                table: "Jogos",
                columns: new[] { "IdJogo", "EmDestaque", "Genero", "Imagem", "Preco", "Titulo" },
                values: new object[,]
                {
                    { 1, true, "Aventura / Sobrevivência", "img/jogos/green-hell.jpg", 149.99m, "Green Hell" },
                    { 2, true, "Estratégia / Casual", "img/jogos/plants-vs-zombies.jpg", 79.50m, "Plants vs. Zombies" },
                    { 3, true, "Arcade / VR", "img/jogos/lazerbait.jpg", 99.00m, "LazerBait" },
                    { 4, true, "Terror / Indie", "img/jogos/slender-the-arrival.jpg", 119.99m, "Slender: The Arrival" },
                    { 5, true, "Ação / Roguelite", "img/jogos/hordes-of-hunger.jpg", 89.90m, "Hordes of Hunger" },
                    { 6, true, "Luta / Ação", "img/jogos/brawlhalla.jpg", null, "Brawlhalla" },
                    { 7, false, "Terror / Cooperativo", "img/jogos/pacify.jpg", 109.90m, "Pacify" },
                    { 8, false, "Simulação / Indie", "img/jogos/supermarket-simulator.jpg", 65.00m, "Supermarket Simulator" },
                    { 9, false, "Estratégia / Indie", "img/jogos/steam-tactics.jpg", 129.99m, "Steam Tactics" },
                    { 10, false, "RPG / Indie", "img/jogos/old-school-rpg.jpg", 179.90m, "Old School RPG" },
                    { 11, false, "Ação / Aventura", "img/jogos/red-dead-redemption-2.webp", 299.00m, "Red Dead Redemption 2" },
                    { 12, false, "Esporte / Futebol", "img/jogos/ea-sports-fc-26.jpg", 349.99m, "EA SPORTS FC 26" },
                    { 13, false, "Ação / Tiro", "img/jogos/call-of-duty-black-ops-7.jpg", 349.90m, "Call of Duty: Black Ops 7" },
                    { 14, false, "Corrida / Simulação", "img/jogos/project-motor-racing.jpg", 249.90m, "Project Motor Racing" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Interacoes_IdChamado",
                table: "Interacoes",
                column: "IdChamado");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chamados_IdCliente",
                table: "Chamados",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Chamados_IdFuncionarioResponsavel",
                table: "Chamados",
                column: "IdFuncionarioResponsavel");

            migrationBuilder.CreateIndex(
                name: "IX_Chamados_Status",
                table: "Chamados",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Chamados_Clientes_IdCliente",
                table: "Chamados",
                column: "IdCliente",
                principalTable: "Clientes",
                principalColumn: "IdCliente",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chamados_Funcionarios_IdFuncionarioResponsavel",
                table: "Chamados",
                column: "IdFuncionarioResponsavel",
                principalTable: "Funcionarios",
                principalColumn: "IdFuncionario",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Interacoes_Chamados_IdChamado",
                table: "Interacoes",
                column: "IdChamado",
                principalTable: "Chamados",
                principalColumn: "IdChamado",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chamados_Clientes_IdCliente",
                table: "Chamados");

            migrationBuilder.DropForeignKey(
                name: "FK_Chamados_Funcionarios_IdFuncionarioResponsavel",
                table: "Chamados");

            migrationBuilder.DropForeignKey(
                name: "FK_Interacoes_Chamados_IdChamado",
                table: "Interacoes");

            migrationBuilder.DropTable(
                name: "Jogos");

            migrationBuilder.DropIndex(
                name: "IX_Interacoes_IdChamado",
                table: "Interacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Email",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Chamados_IdCliente",
                table: "Chamados");

            migrationBuilder.DropIndex(
                name: "IX_Chamados_IdFuncionarioResponsavel",
                table: "Chamados");

            migrationBuilder.DropIndex(
                name: "IX_Chamados_Status",
                table: "Chamados");

            migrationBuilder.DropColumn(
                name: "IdChamado",
                table: "Interacoes");

            migrationBuilder.DropColumn(
                name: "NomeAutor",
                table: "Interacoes");

            migrationBuilder.DropColumn(
                name: "TipoAutor",
                table: "Interacoes");

            migrationBuilder.DropColumn(
                name: "IdCliente",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DataCadastro",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DataAtualizacao",
                table: "Chamados");

            migrationBuilder.DropColumn(
                name: "IdCliente",
                table: "Chamados");

            migrationBuilder.DropColumn(
                name: "IdFuncionarioResponsavel",
                table: "Chamados");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Chamados");

            migrationBuilder.AlterColumn<string>(
                name: "Mensagem",
                table: "Interacoes",
                type: "nvarchar(700)",
                maxLength: 700,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataHora",
                table: "Interacoes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Autor",
                table: "Interacoes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Telefone",
                table: "Funcionarios",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SenhaHash",
                table: "Funcionarios",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataNascimento",
                table: "Funcionarios",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "SenhaHash",
                table: "Clientes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataNascimento",
                table: "Clientes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Chamados",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataInicio",
                table: "Chamados",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "EmailUsuario",
                table: "Chamados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusChamado",
                table: "Chamados",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes",
                column: "Email");
        }
    }
}
