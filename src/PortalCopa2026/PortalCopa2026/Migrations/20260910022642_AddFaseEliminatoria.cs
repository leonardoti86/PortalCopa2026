using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalCopa2026.Migrations
{
    /// <inheritdoc />
    public partial class AddFaseEliminatoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JogoOrigemMandanteId",
                table: "Jogos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JogoOrigemVisitanteId",
                table: "Jogos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ordem",
                table: "Jogos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlacarPenaltisMandante",
                table: "Jogos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlacarPenaltisVisitante",
                table: "Jogos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_JogoOrigemMandanteId",
                table: "Jogos",
                column: "JogoOrigemMandanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_JogoOrigemVisitanteId",
                table: "Jogos",
                column: "JogoOrigemVisitanteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jogos_Jogos_JogoOrigemMandanteId",
                table: "Jogos",
                column: "JogoOrigemMandanteId",
                principalTable: "Jogos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jogos_Jogos_JogoOrigemVisitanteId",
                table: "Jogos",
                column: "JogoOrigemVisitanteId",
                principalTable: "Jogos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jogos_Jogos_JogoOrigemMandanteId",
                table: "Jogos");

            migrationBuilder.DropForeignKey(
                name: "FK_Jogos_Jogos_JogoOrigemVisitanteId",
                table: "Jogos");

            migrationBuilder.DropIndex(
                name: "IX_Jogos_JogoOrigemMandanteId",
                table: "Jogos");

            migrationBuilder.DropIndex(
                name: "IX_Jogos_JogoOrigemVisitanteId",
                table: "Jogos");

            migrationBuilder.DropColumn(
                name: "JogoOrigemMandanteId",
                table: "Jogos");

            migrationBuilder.DropColumn(
                name: "JogoOrigemVisitanteId",
                table: "Jogos");

            migrationBuilder.DropColumn(
                name: "Ordem",
                table: "Jogos");

            migrationBuilder.DropColumn(
                name: "PlacarPenaltisMandante",
                table: "Jogos");

            migrationBuilder.DropColumn(
                name: "PlacarPenaltisVisitante",
                table: "Jogos");
        }
    }
}
