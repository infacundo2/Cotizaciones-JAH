using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaCotizaciones.Web.Migrations
{
    /// <inheritdoc />
    public partial class ClienteRegionOpciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Clientes",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CategoriasCotizacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasCotizacion", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FormasPagoCotizacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormasPagoCotizacion", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasCotizacion_Nombre",
                table: "CategoriasCotizacion",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormasPagoCotizacion_Nombre",
                table: "FormasPagoCotizacion",
                column: "Nombre",
                unique: true);

            migrationBuilder.InsertData(
                table: "CategoriasCotizacion",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Mantencion" },
                    { 2, "Reparacion" },
                    { 3, "Instalacion" },
                    { 4, "Servicio tecnico" },
                    { 5, "Repuestos" }
                });

            migrationBuilder.InsertData(
                table: "FormasPagoCotizacion",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Efectivo" },
                    { 2, "Transferencia" },
                    { 3, "Cheque" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CategoriasCotizacion");
            migrationBuilder.DropTable(name: "FormasPagoCotizacion");
            migrationBuilder.DropColumn(name: "Region", table: "Clientes");
        }
    }
}
