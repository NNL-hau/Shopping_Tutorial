using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Tutorial.Migrations
{
    /// <inheritdoc />
    public partial class AddContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Map = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoImg = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Name);
                });

            migrationBuilder.InsertData(
               table: "Contact",
               columns: new[] { "Name", "Map", "Email", "Phone", "Description", "LogoImg" },
               values: new object[,]
               {
                    {
                        "Cửa hàng QH",
                        "<iframe src=\"https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3723.9419792062915!2d105.78658!3d20.9805574!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3135ade83ba9e115%3A0x6f4fdb5e1e9e39ed!2zVHLGsOG7nW5nIMSQ4bqhaSBo4buNYyBLaeG7h24gdHLhu7FjIEjDoCBO4buZaQ!5e0!3m2!1svi!2s!4v1697999999999!5m2!1svi!2s\" width=\"100%\" height=\"300\" style=\"border:0;\" allowfullscreen=\"\" loading=\"lazy\" referrerpolicy=\"no-referrer-when-downgrade\"></iframe>",
                        "anhhung9hot@gmail.com",
                        "0334626089",
                        "Cửa hàng trực tiếp",
                        "/images/6.jpg"
                    }
               });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contact");
        }
    }
}
