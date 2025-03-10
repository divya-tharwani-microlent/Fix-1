using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zoho_timesheet_EFC.Migrations
{
    /// <inheritdoc />
    public partial class addsourcetbl_changesinTimesheet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InternalTaskId",
                table: "Timesheets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                table: "Timesheets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Timesheets_InternalTaskId",
                table: "Timesheets",
                column: "InternalTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Timesheets_SourceId",
                table: "Timesheets",
                column: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Timesheets_Sources_SourceId",
                table: "Timesheets",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Timesheets_TaskDetails_InternalTaskId",
                table: "Timesheets",
                column: "InternalTaskId",
                principalTable: "TaskDetails",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Timesheets_Sources_SourceId",
                table: "Timesheets");

            migrationBuilder.DropForeignKey(
                name: "FK_Timesheets_TaskDetails_InternalTaskId",
                table: "Timesheets");

            migrationBuilder.DropTable(
                name: "Sources");

            migrationBuilder.DropIndex(
                name: "IX_Timesheets_InternalTaskId",
                table: "Timesheets");

            migrationBuilder.DropIndex(
                name: "IX_Timesheets_SourceId",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "InternalTaskId",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "Timesheets");
        }
    }
}
