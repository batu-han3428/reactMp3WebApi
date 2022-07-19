using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TekrarApp.Migrations
{
    public partial class confirmEmailToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmEmailToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmEmailToken",
                table: "Users");
        }
    }
}
