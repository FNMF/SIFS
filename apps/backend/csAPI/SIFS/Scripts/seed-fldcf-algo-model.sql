-- Seed the default FLDCF algorithm endpoint.
-- The backend now resolves algorithm API URLs only from algo_models.

SET @OLD_SQL_MODE := @@SESSION.SQL_MODE;
SET SESSION SQL_MODE = CONCAT_WS(',', @@SESSION.SQL_MODE, 'NO_AUTO_VALUE_ON_ZERO');

START TRANSACTION;

DELETE FROM algo_models
WHERE id = 0 AND name <> 'FLDCF';

UPDATE algo_models
SET id = 0
WHERE name = 'FLDCF'
  AND NOT EXISTS (
      SELECT 1
      FROM (SELECT id FROM algo_models WHERE id = 0) AS existing_zero_id
  );

INSERT INTO algo_models
    (id, name, enabled, api_url, description, reserved_json, created_at, updated_at, deleted_at)
VALUES
    (0, 'FLDCF', TRUE, 'http://127.0.0.1:8000/detect/fldcf', 'Default FLDCF algorithm endpoint', NULL, UTC_TIMESTAMP(), UTC_TIMESTAMP(), NULL)
ON DUPLICATE KEY UPDATE
    enabled = VALUES(enabled),
    api_url = VALUES(api_url),
    description = VALUES(description),
    deleted_at = NULL,
    updated_at = UTC_TIMESTAMP();

COMMIT;

SET SESSION SQL_MODE = @OLD_SQL_MODE;
