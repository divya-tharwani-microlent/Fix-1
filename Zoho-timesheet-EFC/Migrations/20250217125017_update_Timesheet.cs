using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zoho_timesheet_EFC.Migrations
{
    /// <inheritdoc />
    public partial class update_Timesheet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskNameorCategory",
                table: "Timesheets");

            migrationBuilder.AddColumn<string>(
                name: "TaskName",
                table: "Timesheets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskName",
                table: "Timesheets");

            migrationBuilder.AddColumn<string>(
                name: "TaskNameorCategory",
                table: "Timesheets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
