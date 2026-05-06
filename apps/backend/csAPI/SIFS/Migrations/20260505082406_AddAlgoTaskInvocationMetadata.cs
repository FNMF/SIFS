using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIFS.Migrations
{
    /// <inheritdoc />
    public partial class AddAlgoTaskInvocationMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "algo_api_url",
                table: "algo_task",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "algo_model_id",
                table: "algo_task",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "algo_name",
                table: "algo_task",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "failure_reason",
                table: "algo_task",
                type: "text",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_algo_task_algo_model_id",
                table: "algo_task",
                column: "algo_model_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_algo_task_algo_model_id",
                table: "algo_task");

            migrationBuilder.DropColumn(
                name: "algo_api_url",
                table: "algo_task");

            migrationBuilder.DropColumn(
                name: "algo_model_id",
                table: "algo_task");

            migrationBuilder.DropColumn(
                name: "algo_name",
                table: "algo_task");

            migrationBuilder.DropColumn(
                name: "failure_reason",
                table: "algo_task");
        }
    }
}
