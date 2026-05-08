-- Baseline algorithm metadata.
-- Safe to run repeatedly.
-- Algorithm choices are driven by algo_models. The legacy algo_type table is not seeded.

SET @OLD_SQL_MODE := @@SESSION.SQL_MODE;
SET SESSION SQL_MODE = CONCAT_WS(',', @@SESSION.SQL_MODE, 'NO_AUTO_VALUE_ON_ZERO');

INSERT INTO algo_models
    (id, name, enabled, api_url, description, reserved_json, created_at, updated_at, deleted_at)
VALUES
    (0, 'FLDCF', TRUE, 'http://127.0.0.1:8000/detect/fldcf', 'Default FLDCF algorithm endpoint', NULL, UTC_TIMESTAMP(), UTC_TIMESTAMP(), NULL),
    (1, 'FECDNet', TRUE, 'http://127.0.0.1:8001/detect/fecdnet', 'Default FECDNet algorithm endpoint', NULL, UTC_TIMESTAMP(), UTC_TIMESTAMP(), NULL)
ON DUPLICATE KEY UPDATE
    name = VALUES(name),
    enabled = VALUES(enabled),
    api_url = VALUES(api_url),
    description = VALUES(description),
    deleted_at = NULL,
    updated_at = UTC_TIMESTAMP();

SET SESSION SQL_MODE = @OLD_SQL_MODE;
