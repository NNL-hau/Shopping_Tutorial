using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Tutorial.Migrations
{
    /// <inheritdoc />
    public partial class Add3DModelsAndAnnotations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfigurationData",
                table: "UserCartItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfigurationId",
                table: "UserCartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Product3DModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductID = table.Column<long>(type: "bigint", nullable: false),
                    Model3DPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TexturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultScale = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CameraPositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CameraPositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CameraPositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SupportAR = table.Column<bool>(type: "bit", nullable: false),
                    SupportVR = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product3DModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product3DModels_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductAnnotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductID = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarkerColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAnnotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAnnotations_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductID = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelectedColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SelectedMaterial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SelectedComponents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ConfigurationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsInCart = table.Column<bool>(type: "bit", nullable: false),
                    CartItemId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductConfigurations_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCartItems_ConfigurationId",
                table: "UserCartItems",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Product3DModels_ProductID",
                table: "Product3DModels",
                column: "ProductID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAnnotations_ProductID",
                table: "ProductAnnotations",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductConfigurations_ProductID",
                table: "ProductConfigurations",
                column: "ProductID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCartItems_ProductConfigurations_ConfigurationId",
                table: "UserCartItems",
                column: "ConfigurationId",
                principalTable: "ProductConfigurations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCartItems_ProductConfigurations_ConfigurationId",
                table: "UserCartItems");

            migrationBuilder.DropTable(
                name: "Product3DModels");

            migrationBuilder.DropTable(
                name: "ProductAnnotations");

            migrationBuilder.DropTable(
                name: "ProductConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_UserCartItems_ConfigurationId",
                table: "UserCartItems");

            migrationBuilder.DropColumn(
                name: "ConfigurationData",
                table: "UserCartItems");

            migrationBuilder.DropColumn(
                name: "ConfigurationId",
                table: "UserCartItems");
        }
    }
}
