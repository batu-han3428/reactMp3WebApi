using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TekrarApp.Migrations
{
    public partial class initDb3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmEmail",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsConfirmEmail",
                table: "Users");
        }
    }
}
