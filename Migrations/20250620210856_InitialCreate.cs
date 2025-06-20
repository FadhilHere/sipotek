using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SIPOTEK.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Obats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NamaObat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JenisObat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BentukObat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Harga = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Stok = table.Column<int>(type: "int", nullable: false),
                    TglKadaluarsa = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObatKeluars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObatId = table.Column<int>(type: "int", nullable: false),
                    JumlahKeluar = table.Column<int>(type: "int", nullable: false),
                    TotalHarga = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TglKeluar = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Pelanggan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NoTransaksi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObatKeluars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObatKeluars_Obats_ObatId",
                        column: x => x.ObatId,
                        principalTable: "Obats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObatMasuks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObatId = table.Column<int>(type: "int", nullable: false),
                    JumlahMasuk = table.Column<int>(type: "int", nullable: false),
                    TotalHarga = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TglKadaluarsaM = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TglMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObatMasuks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObatMasuks_Obats_ObatId",
                        column: x => x.ObatId,
                        principalTable: "Obats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Obats",
                columns: new[] { "Id", "BentukObat", "Harga", "JenisObat", "NamaObat", "Stok", "TglKadaluarsa" },
                values: new object[,]
                {
                    { 1, "Tablet", 6000m, "Analgesik", "Paracetamol", 20, new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "Kapsul", 8000m, "Antibiotik", "Amoxicillin", 30, new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "Sirup", 18000m, "Ekspektoran", "OBH", 40, new DateTime(2026, 6, 17, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ObatKeluars_ObatId",
                table: "ObatKeluars",
                column: "ObatId");

            migrationBuilder.CreateIndex(
                name: "IX_ObatMasuks_ObatId",
                table: "ObatMasuks",
                column: "ObatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObatKeluars");

            migrationBuilder.DropTable(
                name: "ObatMasuks");

            migrationBuilder.DropTable(
                name: "Obats");
        }
    }
}
