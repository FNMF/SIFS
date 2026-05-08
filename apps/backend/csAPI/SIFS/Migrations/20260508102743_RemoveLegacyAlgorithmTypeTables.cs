using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIFS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyAlgorithmTypeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS `task_type_map`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `algo_type`;");
            migrationBuilder.Sql(@"
                SET @drop_algo_type_column := (
                    SELECT IF(
                        EXISTS(
                            SELECT 1
                            FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_SCHEMA = DATABASE()
                              AND TABLE_NAME = 'result_file'
                              AND COLUMN_NAME = 'algo_type'
                        ),
                        'ALTER TABLE `result_file` DROP COLUMN `algo_type`',
                        'SELECT 1'
                    )
                );
                PREPARE stmt FROM @drop_algo_type_column;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "algo_type",
                table: "result_file",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "algo_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "task_type_map",
                columns: table => new
                {
                    id = table.Column<byte[]>(type: "binary(16)", fixedLength: true, maxLength: 16, nullable: false),
                    task_id = table.Column<byte[]>(type: "binary(16)", fixedLength: true, maxLength: 16, nullable: false),
                    type_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");
        }
    }
}
