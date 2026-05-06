using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIFS.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "task_list",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "algo_task",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "finished_at",
                table: "algo_task",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "started_at",
                table: "algo_task",
                type: "datetime",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_task_list_created_at",
                table: "task_list",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_task_list_deleted_at",
                table: "task_list",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_task_list_user_id",
                table: "task_list",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_algo_task_deleted_at",
                table: "algo_task",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_algo_task_finished_at",
                table: "algo_task",
                column: "finished_at");

            migrationBuilder.CreateIndex(
                name: "ix_algo_task_started_at",
                table: "algo_task",
                column: "started_at");

            migrationBuilder.CreateIndex(
                name: "ix_algo_task_status",
                table: "algo_task",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_task_list_created_at",
                table: "task_list");

            migrationBuilder.DropIndex(
                name: "ix_task_list_deleted_at",
                table: "task_list");

            migrationBuilder.DropIndex(
                name: "ix_task_list_user_id",
                table: "task_list");

            migrationBuilder.DropIndex(
                name: "ix_algo_task_deleted_at",
                table: "algo_task");

            migrationBuilder.DropIndex(
                name: "ix_algo_task_finished_at",
                table: "algo_task");

            migrationBuilder.DropIndex(
                name: "ix_algo_task_started_at",
                table: "algo_task");

            migrationBuilder.DropIndex(
                name: "ix_algo_task_status",
                table: "algo_task");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "task_list");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "algo_task");

            migrationBuilder.DropColumn(
                name: "finished_at",
                table: "algo_task");

            migrationBuilder.DropColumn(
                name: "started_at",
                table: "algo_task");
        }
    }
}
