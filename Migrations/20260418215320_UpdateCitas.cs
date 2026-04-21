using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoDeGradoFundacion.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCitas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado_cita",
                table: "citas",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_analisis",
                table: "citas",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "grado_retinopatia",
                table: "citas",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "observaciones_ojo_derecho",
                table: "citas",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "observaciones_ojo_izquierdo",
                table: "citas",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "resultado_ojo_derecho",
                table: "citas",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "resultado_ojo_izquierdo",
                table: "citas",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "tipo_retinopatia",
                table: "citas",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado_cita",
                table: "citas");

            migrationBuilder.DropColumn(
                name: "fecha_analisis",
                table: "citas");

            migrationBuilder.DropColumn(
                name: "grado_retinopatia",
                table: "citas");

            migrationBuilder.DropColumn(
                name: "observaciones_ojo_derecho",
                table: "citas");

            migrationBuilder.DropColumn(
                name: "observaciones_ojo_izquierdo",
                table: "citas");

            migrationBuilder.DropColumn(
                name: "resultado_ojo_derecho",
                table: "citas");

            migrationBuilder.DropColumn(
                name: "resultado_ojo_izquierdo",
                table: "citas");

            migrationBuilder.DropColumn(
                name: "tipo_retinopatia",
                table: "citas");
        }
    }
}
