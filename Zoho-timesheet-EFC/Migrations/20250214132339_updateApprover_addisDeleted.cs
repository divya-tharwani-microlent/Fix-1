using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zoho_timesheet_EFC.Migrations
{
    /// <inheritdoc />
    public partial class updateApprover_addisDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Approvers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Approvers");
        }
    }
}
