using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Tutorial.Migrations
{
    /// <inheritdoc />
    public partial class fixCoupon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Coupons",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_UserId",
                table: "Coupons",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_AspNetUsers_UserId",
                table: "Coupons",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_AspNetUsers_UserId",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_UserId",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Coupons");
        }
    }
}
