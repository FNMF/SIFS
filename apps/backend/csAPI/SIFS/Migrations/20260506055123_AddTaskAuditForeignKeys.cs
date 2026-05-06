using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIFS.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskAuditForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_task_audits_task_list_task_id",
                table: "task_audits",
                column: "task_id",
                principalTable: "task_list",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_task_audits_user_operator_id",
                table: "task_audits",
                column: "operator_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_task_audits_task_list_task_id",
                table: "task_audits");

            migrationBuilder.DropForeignKey(
                name: "FK_task_audits_user_operator_id",
                table: "task_audits");
        }
    }
}
