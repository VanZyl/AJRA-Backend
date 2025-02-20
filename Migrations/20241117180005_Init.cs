using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AJRAApis.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Leave = table.Column<float>(type: "real", nullable: false),
                    SickLeave = table.Column<float>(type: "real", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxRefNum = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaySlips",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NormalHoursWorked = table.Column<float>(type: "real", nullable: false),
                    NormalAmountPaid = table.Column<float>(type: "real", nullable: false),
                    OvertimeHoursWorked = table.Column<float>(type: "real", nullable: false),
                    OvertimeAmountPaid = table.Column<float>(type: "real", nullable: false),
                    PublicHolidayHoursWorked = table.Column<float>(type: "real", nullable: false),
                    PublicHolidayAmountPaid = table.Column<float>(type: "real", nullable: false),
                    LeaveHoursWorked = table.Column<float>(type: "real", nullable: false),
                    LeaveAmountPaid = table.Column<float>(type: "real", nullable: false),
                    GrossAmount = table.Column<float>(type: "real", nullable: false),
                    UIFContribution = table.Column<float>(type: "real", nullable: false),
                    BarganingCouncil = table.Column<float>(type: "real", nullable: false),
                    Uniforms = table.Column<float>(type: "real", nullable: false),
                    TillShortage = table.Column<float>(type: "real", nullable: false),
                    Wastages = table.Column<float>(type: "real", nullable: false),
                    OtherDeductions = table.Column<float>(type: "real", nullable: false),
                    NetAmount = table.Column<float>(type: "real", nullable: false),
                    LeaveBF = table.Column<float>(type: "real", nullable: false),
                    LeaveAcc = table.Column<float>(type: "real", nullable: false),
                    LeaveTaken = table.Column<float>(type: "real", nullable: false),
                    SickLeaveTaken = table.Column<float>(type: "real", nullable: false),
                    OtherTaken = table.Column<float>(type: "real", nullable: false),
                    PaySlipCycle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaySlipDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaySlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaySlips_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaySlips_EmployeeId",
                table: "PaySlips",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaySlips");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
