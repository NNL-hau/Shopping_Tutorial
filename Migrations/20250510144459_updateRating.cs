using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Tutorial.Migrations
{
    /// <inheritdoc />
    public partial class updateRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xóa index nếu đã tồn tại (unique)
            migrationBuilder.DropIndex(
                name: "IX_Ratings_ProductID",
                table: "Ratings");

            // Tạo lại index không unique
            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ProductID",
                table: "Ratings",
                column: "ProductID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa index không unique
            migrationBuilder.DropIndex(
                name: "IX_Ratings_ProductID",
                table: "Ratings");

            // Tạo lại index unique như ban đầu
            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ProductID",
                table: "Ratings",
                column: "ProductID",
                unique: true);
        }
    }
}
