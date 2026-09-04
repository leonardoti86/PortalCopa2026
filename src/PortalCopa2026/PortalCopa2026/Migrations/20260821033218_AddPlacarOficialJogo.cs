using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalCopa2026.Migrations
{
    /// <inheritdoc />
    public partial class AddPlacarOficialJogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlacarMandante",
                table: "Jogos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlacarVisitante",
                table: "Jogos",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlacarMandante",
                table: "Jogos");

            migrationBuilder.DropColumn(
                name: "PlacarVisitante",
                table: "Jogos");
        }
    }
}
