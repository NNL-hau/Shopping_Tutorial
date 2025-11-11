using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Tutorial.Migrations
{
    public partial class dateCreated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "VnInfors", // Sửa lại tên bảng nếu cần
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow); // Nếu muốn giá trị mặc định là thời gian hiện tại
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "VnInfors"); // Sửa lại tên bảng nếu cần
        }
    }
}
