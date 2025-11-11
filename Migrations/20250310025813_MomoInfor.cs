using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Tutorial.Migrations
{
    /// <inheritdoc />
    public partial class MomoInfor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Profit",
                table: "Statisticals",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(16,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)");

            migrationBuilder.CreateTable(
                name: "MomoInfors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderInfor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DatePaid = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MomoInfors", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MomoInfors");

            migrationBuilder.AlterColumn<int>(
                name: "Profit",
                table: "Statisticals",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(8,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(16,2)");
        }
    }
}
