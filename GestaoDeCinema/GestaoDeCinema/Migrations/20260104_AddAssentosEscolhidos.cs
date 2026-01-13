using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoDeCinema.Migrations
{
    /// <inheritdoc />
    public partial class AddAssentosEscolhidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adicionar coluna AssentosEscolhidos à tabela Reservas
            migrationBuilder.AddColumn<string>(
                name: "AssentosEscolhidos",
                table: "Reservas",
                type: "TEXT",
                nullable: true);

            // Garantir que as colunas Sala e Capacidade existem em Sessoes
            // (se já existirem, a operação será ignorada)
            try
            {
                migrationBuilder.AddColumn<int>(
                    name: "Sala",
                    table: "Sessoes",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 1);
            }
            catch { }

            try
            {
                migrationBuilder.AddColumn<int>(
                    name: "Capacidade",
                    table: "Sessoes",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 100);
            }
            catch { }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssentosEscolhidos",
                table: "Reservas");
        }
    }
}
