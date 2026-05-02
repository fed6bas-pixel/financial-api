using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Malia.Migrations
{
    public partial class AddFullNameToEmployee_v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Fullname",
                table: "Employees",
                newName: "FullName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Employees",
                newName: "Fullname");
        }
    }
}
