using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIFS.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeDefaultAlgoModelIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET @old_fldcf_id := (SELECT id FROM algo_models WHERE name = 'FLDCF' LIMIT 1);
                SET @old_fecdnet_id := (SELECT id FROM algo_models WHERE name = 'FECDNet' LIMIT 1);
                SET @old_foreign_key_checks := @@FOREIGN_KEY_CHECKS;
                SET FOREIGN_KEY_CHECKS = 0;

                UPDATE algo_models
                SET id = -1000001
                WHERE id = 1 AND name = 'FECDNet';

                UPDATE algo_task
                SET algo_model_id = -1000001
                WHERE algo_model_id = 1 AND @old_fecdnet_id = 1;

                UPDATE model_health_checks
                SET algo_model_id = -1000001
                WHERE algo_model_id = 1 AND @old_fecdnet_id = 1;

                UPDATE operation_logs
                SET target_id = '-1000001'
                WHERE target_type = 'algo' AND target_id = '1' AND @old_fecdnet_id = 1;

                UPDATE algo_models
                SET id = 1
                WHERE name = 'FLDCF';

                UPDATE algo_task
                SET algo_model_id = 1
                WHERE @old_fldcf_id IS NOT NULL AND algo_model_id = @old_fldcf_id;

                UPDATE model_health_checks
                SET algo_model_id = 1
                WHERE @old_fldcf_id IS NOT NULL AND algo_model_id = @old_fldcf_id;

                UPDATE operation_logs
                SET target_id = '1'
                WHERE @old_fldcf_id IS NOT NULL
                  AND target_type = 'algo'
                  AND target_id = CAST(@old_fldcf_id AS CHAR);

                UPDATE algo_models
                SET id = 2
                WHERE id = -1000001 AND name = 'FECDNet';

                UPDATE algo_task
                SET algo_model_id = 2
                WHERE algo_model_id = -1000001;

                UPDATE model_health_checks
                SET algo_model_id = 2
                WHERE algo_model_id = -1000001;

                UPDATE operation_logs
                SET target_id = '2'
                WHERE target_type = 'algo' AND target_id = '-1000001';

                INSERT INTO algo_models
                    (id, name, enabled, api_url, description, reserved_json, created_at, updated_at, deleted_at)
                SELECT
                    1, 'FLDCF', TRUE, 'http://127.0.0.1:8000/detect/fldcf', 'Default FLDCF algorithm endpoint', NULL, UTC_TIMESTAMP(), UTC_TIMESTAMP(), NULL
                WHERE NOT EXISTS (SELECT 1 FROM algo_models WHERE name = 'FLDCF');

                INSERT INTO algo_models
                    (id, name, enabled, api_url, description, reserved_json, created_at, updated_at, deleted_at)
                SELECT
                    2, 'FECDNet', TRUE, 'http://127.0.0.1:8001/detect/fecdnet', 'Default FECDNet algorithm endpoint', NULL, UTC_TIMESTAMP(), UTC_TIMESTAMP(), NULL
                WHERE NOT EXISTS (SELECT 1 FROM algo_models WHERE name = 'FECDNet');

                SET FOREIGN_KEY_CHECKS = @old_foreign_key_checks;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET @old_sql_mode := @@SESSION.SQL_MODE;
                SET SESSION SQL_MODE = CONCAT_WS(',', @@SESSION.SQL_MODE, 'NO_AUTO_VALUE_ON_ZERO');
                SET @old_foreign_key_checks := @@FOREIGN_KEY_CHECKS;
                SET FOREIGN_KEY_CHECKS = 0;

                UPDATE algo_models
                SET id = -1000001
                WHERE id = 2 AND name = 'FECDNet';

                UPDATE algo_task
                SET algo_model_id = -1000001
                WHERE algo_model_id = 2;

                UPDATE model_health_checks
                SET algo_model_id = -1000001
                WHERE algo_model_id = 2;

                UPDATE operation_logs
                SET target_id = '-1000001'
                WHERE target_type = 'algo' AND target_id = '2';

                UPDATE algo_models
                SET id = 0
                WHERE id = 1 AND name = 'FLDCF';

                UPDATE algo_task
                SET algo_model_id = 0
                WHERE algo_model_id = 1;

                UPDATE model_health_checks
                SET algo_model_id = 0
                WHERE algo_model_id = 1;

                UPDATE operation_logs
                SET target_id = '0'
                WHERE target_type = 'algo' AND target_id = '1';

                UPDATE algo_models
                SET id = 1
                WHERE id = -1000001 AND name = 'FECDNet';

                UPDATE algo_task
                SET algo_model_id = 1
                WHERE algo_model_id = -1000001;

                UPDATE model_health_checks
                SET algo_model_id = 1
                WHERE algo_model_id = -1000001;

                UPDATE operation_logs
                SET target_id = '1'
                WHERE target_type = 'algo' AND target_id = '-1000001';

                SET FOREIGN_KEY_CHECKS = @old_foreign_key_checks;
                SET SESSION SQL_MODE = @old_sql_mode;
                """);
        }
    }
}
