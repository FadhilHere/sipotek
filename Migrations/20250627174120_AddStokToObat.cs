using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIPOTEK.Migrations
{
    /// <inheritdoc />
    public partial class AddStokToObat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StokMinimum",
                table: "Obats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Obats",
                keyColumn: "Id",
                keyValue: 1,
                column: "StokMinimum",
                value: 10);

            migrationBuilder.UpdateData(
                table: "Obats",
                keyColumn: "Id",
                keyValue: 2,
                column: "StokMinimum",
                value: 10);

            migrationBuilder.UpdateData(
                table: "Obats",
                keyColumn: "Id",
                keyValue: 3,
                column: "StokMinimum",
                value: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StokMinimum",
                table: "Obats");
        }
    }
}
