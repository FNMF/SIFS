-- Docker core deployment default algorithm endpoints.
-- Algorithm choices are driven by algo_models. The legacy algo_type table is not seeded.
-- In core-only deployment, algorithm APIs are external services from the backend container's perspective.
-- On Docker Desktop, host.docker.internal points to the host machine. For remote/third-party APIs,
-- update algo_models.api_url in the admin UI or database.

INSERT INTO algo_models
    (id, name, enabled, api_url, description, reserved_json, created_at, updated_at, deleted_at)
VALUES
    (1, 'FLDCF', TRUE, 'http://host.docker.internal:8000/detect/fldcf', 'Default FLDCF algorithm endpoint', NULL, UTC_TIMESTAMP(), UTC_TIMESTAMP(), NULL),
    (2, 'FECDNet', TRUE, 'http://host.docker.internal:8001/detect/fecdnet', 'Default FECDNet algorithm endpoint', NULL, UTC_TIMESTAMP(), UTC_TIMESTAMP(), NULL)
ON DUPLICATE KEY UPDATE
    name = VALUES(name),
    enabled = VALUES(enabled),
    api_url = VALUES(api_url),
    description = VALUES(description),
    deleted_at = NULL,
    updated_at = UTC_TIMESTAMP();
