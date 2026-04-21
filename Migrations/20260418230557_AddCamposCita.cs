using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoDeGradoFundacion.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "confianza_ojo_derecho",
                table: "citas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "confianza_ojo_izquierdo",
                table: "citas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "recomendacion_modelo",
                table: "citas",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "confianza_ojo_derecho",
                table: "citas");

            migrationBuilder.DropColumn(
                name: "confianza_ojo_izquierdo",
                table: "citas");

            migrationBuilder.DropColumn(
                name: "recomendacion_modelo",
                table: "citas");
        }
    }
}
