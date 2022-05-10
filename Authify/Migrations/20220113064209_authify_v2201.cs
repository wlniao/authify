using Microsoft.EntityFrameworkCore.Migrations;

namespace Authify.Migrations
{
    public partial class authify_v2201 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "jsonstr",
                table: "authify_auth",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "authify_accesstoken",
                columns: table => new
                {
                    key = table.Column<string>(maxLength: 50, nullable: false),
                    token = table.Column<string>(maxLength: 512, nullable: true),
                    expires_in = table.Column<long>(nullable: false),
                    response = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authify_accesstoken", x => x.key);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "authify_accesstoken");

            migrationBuilder.DropColumn(
                name: "jsonstr",
                table: "authify_auth");
        }
    }
}
