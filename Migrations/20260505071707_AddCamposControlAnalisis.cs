using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoDeGradoFundacion.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposControlAnalisis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_carga_ojo_derecho",
                table: "citas",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_carga_ojo_izquierdo",
                table: "citas",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "medico_responsable",
                table: "citas",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_carga_ojo_derecho",
                table: "citas");

            migrationBuilder.DropColumn(
                name: "fecha_carga_ojo_izquierdo",
                table: "citas");

            migrationBuilder.DropColumn(
                name: "medico_responsable",
                table: "citas");
        }
    }
}
