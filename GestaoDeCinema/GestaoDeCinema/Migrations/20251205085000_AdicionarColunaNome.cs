using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoDeCinema.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarColunaNome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "Utilizadores",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nome",
                table: "Utilizadores");
        }
    }
}
