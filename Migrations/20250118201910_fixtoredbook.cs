using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AJRAApis.Migrations
{
    /// <inheritdoc />
    public partial class fixtoredbook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeId",
                table: "Redbook",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Redbook");
        }
    }
}
