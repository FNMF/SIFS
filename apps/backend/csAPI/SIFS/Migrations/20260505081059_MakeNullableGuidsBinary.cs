using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIFS.Migrations
{
    /// <inheritdoc />
    public partial class MakeNullableGuidsBinary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "actor_id",
                table: "operation_logs",
                type: "binary(16)",
                fixedLength: true,
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(16)",
                oldFixedLength: true,
                oldMaxLength: 16,
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "actor_id",
                table: "operation_logs",
                type: "char(16)",
                fixedLength: true,
                maxLength: 16,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(byte[]),
                oldType: "binary(16)",
                oldFixedLength: true,
                oldMaxLength: 16,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
