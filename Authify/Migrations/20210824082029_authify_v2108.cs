using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Authify.Migrations
{
    public partial class authify_v2108 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "authify_account",
                columns: table => new
                {
                    sid = table.Column<string>(maxLength: 50, nullable: false),
                    mobile = table.Column<string>(maxLength: 15, nullable: true),
                    avatar = table.Column<string>(maxLength: 150, nullable: true),
                    avatarsource = table.Column<string>(maxLength: 150, nullable: true),
                    nickname = table.Column<string>(maxLength: 30, nullable: true),
                    truename = table.Column<string>(maxLength: 30, nullable: true),
                    identity = table.Column<string>(maxLength: 25, nullable: true),
                    password = table.Column<string>(maxLength: 100, nullable: true),
                    source = table.Column<string>(maxLength: 30, nullable: true),
                    jointime = table.Column<long>(nullable: false),
                    lastlogin = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authify_account", x => x.sid);
                });

            migrationBuilder.CreateTable(
                name: "authify_auth",
                columns: table => new
                {
                    key = table.Column<string>(maxLength: 50, nullable: false),
                    oid = table.Column<int>(nullable: false),
                    avatar = table.Column<string>(maxLength: 512, nullable: true),
                    nickname = table.Column<string>(maxLength: 50, nullable: true),
                    create = table.Column<long>(nullable: false),
                    update = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authify_auth", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "authify_organ",
                columns: table => new
                {
                    oid = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    secret = table.Column<string>(maxLength: 50, nullable: true),
                    domain = table.Column<string>(maxLength: 50, nullable: true),
                    apiurl = table.Column<string>(maxLength: 100, nullable: true),
                    backurl = table.Column<string>(maxLength: 100, nullable: true),
                    config = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authify_organ", x => x.oid);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "authify_account");

            migrationBuilder.DropTable(
                name: "authify_auth");

            migrationBuilder.DropTable(
                name: "authify_organ");
        }
    }
}
