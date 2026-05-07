using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIFS.Migrations
{
    /// <inheritdoc />
    public partial class AddModelHealthChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "model_health_checks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    algo_model_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    response_time_ms = table.Column<int>(type: "int", nullable: true),
                    checked_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    failure_reason = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_model_health_checks_algo_models_algo_model_id",
                        column: x => x.algo_model_id,
                        principalTable: "algo_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "ix_model_health_checks_algo_model_id",
                table: "model_health_checks",
                column: "algo_model_id");

            migrationBuilder.CreateIndex(
                name: "ix_model_health_checks_checked_at",
                table: "model_health_checks",
                column: "checked_at");

            migrationBuilder.CreateIndex(
                name: "ix_model_health_checks_status",
                table: "model_health_checks",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "model_health_checks");
        }
    }
}
