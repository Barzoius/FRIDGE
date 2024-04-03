using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FINAL_FRIDGE.Data.Migrations
{
    public partial class Notes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Categories",
                type: "nvarchar(110)",
                maxLength: 110,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "Categories");
        }
    }
}
