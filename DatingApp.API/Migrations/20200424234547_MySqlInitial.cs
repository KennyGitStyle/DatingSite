using Microsoft.EntityFrameworkCore.Migrations;

namespace DatingApp.API.Migrations
{
    public partial class MySqlInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SenderDelete",
                table: "Messages",
                newName: "SenderDeleted");

            migrationBuilder.RenameColumn(
                name: "RecipientDelete",
                table: "Messages",
                newName: "RecipientDeleted");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SenderDeleted",
                table: "Messages",
                newName: "SenderDelete");

            migrationBuilder.RenameColumn(
                name: "RecipientDeleted",
                table: "Messages",
                newName: "RecipientDelete");
        }
    }
}
