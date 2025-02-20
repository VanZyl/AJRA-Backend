using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AJRAApis.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusChangeDate",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkStatus",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusChangeDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "WorkStatus",
                table: "Employees");
        }
    }
}
