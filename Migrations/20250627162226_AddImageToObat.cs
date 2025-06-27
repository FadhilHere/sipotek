using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIPOTEK.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToObat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Deskripsi",
                table: "Obats",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GambarFileName",
                table: "Obats",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GambarUrl",
                table: "Obats",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Produsen",
                table: "Obats",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Obats",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Deskripsi", "GambarFileName", "GambarUrl", "Produsen" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Obats",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Deskripsi", "GambarFileName", "GambarUrl", "Produsen" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Obats",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Deskripsi", "GambarFileName", "GambarUrl", "Produsen" },
                values: new object[] { null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deskripsi",
                table: "Obats");

            migrationBuilder.DropColumn(
                name: "GambarFileName",
                table: "Obats");

            migrationBuilder.DropColumn(
                name: "GambarUrl",
                table: "Obats");

            migrationBuilder.DropColumn(
                name: "Produsen",
                table: "Obats");
        }
    }
}
