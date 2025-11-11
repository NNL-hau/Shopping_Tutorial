using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Tutorial.Migrations
{
    /// <inheritdoc />
    public partial class Statisticals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Statisticals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Sold = table.Column<int>(type: "int", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Profit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statisticals", x => x.Id);
                });

            // Thêm dữ liệu mẫu
            migrationBuilder.InsertData(
                table: "Statisticals",
                columns: new[] { "Quantity", "Sold", "Revenue", "Profit", "DateCreated" },
                values: new object[,]
                {
            { 100, 80, 8000m, 2000, new DateTime(2024, 3, 1) },
            { 150, 120, 12000m, 3000, new DateTime(2024, 3, 2) },
            { 200, 170, 17000m, 4000, new DateTime(2024, 3, 3) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statisticals");
        }
    }
}
