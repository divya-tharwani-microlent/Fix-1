using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zoho_timesheet_EFC.Migrations
{
    /// <inheritdoc />
    public partial class addForeignKey_addtblAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "TaskDetails",
                newName: "ProjectTaskId");

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskDetails_ProjectId",
                table: "TaskDetails",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDetails_ProjectTaskId",
                table: "TaskDetails",
                column: "ProjectTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskDetails_ProjectTasks_ProjectTaskId",
                table: "TaskDetails",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskDetails_Projects_ProjectId",
                table: "TaskDetails",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskDetails_ProjectTasks_ProjectTaskId",
                table: "TaskDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskDetails_Projects_ProjectId",
                table: "TaskDetails");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropIndex(
                name: "IX_TaskDetails_ProjectId",
                table: "TaskDetails");

            migrationBuilder.DropIndex(
                name: "IX_TaskDetails_ProjectTaskId",
                table: "TaskDetails");

            migrationBuilder.RenameColumn(
                name: "ProjectTaskId",
                table: "TaskDetails",
                newName: "TaskId");
        }
    }
}
