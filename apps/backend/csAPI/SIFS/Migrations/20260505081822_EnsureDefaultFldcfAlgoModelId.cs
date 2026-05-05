using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIFS.Migrations
{
    /// <inheritdoc />
    public partial class EnsureDefaultFldcfAlgoModelId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET @fldcf_generated_id := (
                    SELECT id FROM algo_models WHERE name = 'FLDCF' LIMIT 1
                );

                DELETE FROM algo_models
                WHERE id = 0 AND name <> 'FLDCF';

                UPDATE algo_models
                SET id = 0
                WHERE name = 'FLDCF' AND @fldcf_generated_id IS NOT NULL;

                INSERT INTO algo_models
                    (id, name, enabled, api_url, description, reserved_json, created_at, updated_at)
                SELECT
                    0, 'FLDCF', TRUE, 'http://127.0.0.1:8000/detect/fldcf', 'Default FLDCF algorithm endpoint', NULL, UTC_TIMESTAMP(), UTC_TIMESTAMP()
                WHERE NOT EXISTS (SELECT 1 FROM algo_models WHERE name = 'FLDCF');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE algo_models
                SET id = 1
                WHERE id = 0 AND name = 'FLDCF';
                """);
        }
    }
}
