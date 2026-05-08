-- Reset all application data while keeping the EF migration history table.
-- Run this only in local/dev environments.

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS role_permission;
DROP TABLE IF EXISTS user_role;
DROP TABLE IF EXISTS permission;
DROP TABLE IF EXISTS role;

TRUNCATE TABLE task_type_map;
TRUNCATE TABLE result_file;
TRUNCATE TABLE localfile;
TRUNCATE TABLE task_audits;
TRUNCATE TABLE algo_task;
TRUNCATE TABLE task_list;
TRUNCATE TABLE model_health_checks;
TRUNCATE TABLE operation_logs;
TRUNCATE TABLE refresh_token;
TRUNCATE TABLE user_roles;
TRUNCATE TABLE role_permissions;
TRUNCATE TABLE permissions;
TRUNCATE TABLE roles;
TRUNCATE TABLE algo_models;
TRUNCATE TABLE algo_type;
TRUNCATE TABLE user;

SET FOREIGN_KEY_CHECKS = 1;
