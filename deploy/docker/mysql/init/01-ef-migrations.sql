CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    CREATE TABLE `roles` (
        `id` int NOT NULL AUTO_INCREMENT,
        `name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `description` varchar(255) CHARACTER SET utf8mb4 NULL,
        `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    CREATE TABLE `permissions` (
        `id` int NOT NULL AUTO_INCREMENT,
        `code` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `description` varchar(255) CHARACTER SET utf8mb4 NULL,
        `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    CREATE TABLE `user_roles` (
        `id` binary(16) NOT NULL,
        `user_id` binary(16) NOT NULL,
        `role_id` int NOT NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
        CONSTRAINT `fk_user_roles_role_id` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE,
        CONSTRAINT `fk_user_roles_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    CREATE TABLE `role_permissions` (
        `id` binary(16) NOT NULL,
        `role_id` int NOT NULL,
        `permission_id` int NOT NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
        CONSTRAINT `fk_role_permissions_permission_id` FOREIGN KEY (`permission_id`) REFERENCES `permissions` (`id`) ON DELETE CASCADE,
        CONSTRAINT `fk_role_permissions_role_id` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    CREATE UNIQUE INDEX `ux_roles_name` ON `roles` (`name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    CREATE UNIQUE INDEX `ux_permissions_code` ON `permissions` (`code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    CREATE INDEX `ix_user_roles_user_id` ON `user_roles` (`user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    CREATE INDEX `ix_user_roles_role_id` ON `user_roles` (`role_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    CREATE UNIQUE INDEX `ux_user_roles_user_id_role_id` ON `user_roles` (`user_id`, `role_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    CREATE INDEX `ix_role_permissions_role_id` ON `role_permissions` (`role_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    CREATE INDEX `ix_role_permissions_permission_id` ON `role_permissions` (`permission_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    CREATE UNIQUE INDEX `ux_role_permissions_role_id_permission_id` ON `role_permissions` (`role_id`, `permission_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    INSERT IGNORE INTO roles (id, name, description, created_at, updated_at) VALUES
    (1, 'admin', 'System administrator', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (2, 'user', 'Default application user', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (3, 'auditor', 'Audit and task review user', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (4, 'algorithm_manager', 'Algorithm model manager', UTC_TIMESTAMP(), UTC_TIMESTAMP());

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    INSERT IGNORE INTO permissions (id, code, description, created_at, updated_at) VALUES
    (1, 'task:view:self', 'View own detection tasks', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (2, 'task:view:all', 'View all detection tasks', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (3, 'task:delete', 'Delete detection tasks', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (4, 'task:retry', 'Retry detection tasks', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (5, 'algo:view', 'View algorithm configuration', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (6, 'algo:create', 'Create algorithm configuration', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (7, 'algo:update', 'Update algorithm configuration', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (8, 'algo:enable', 'Enable or disable algorithms', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (9, 'log:view', 'View operation logs', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (10, 'admin:access', 'Access administration features', UTC_TIMESTAMP(), UTC_TIMESTAMP());

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
    SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
    FROM roles r
    JOIN permissions p
    WHERE r.name = 'admin';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
    SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
    FROM roles r
    JOIN permissions p ON p.code = 'task:view:self'
    WHERE r.name = 'user';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
    SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
    FROM roles r
    JOIN permissions p ON p.code IN ('task:view:all', 'log:view')
    WHERE r.name = 'auditor';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
    SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
    FROM roles r
    JOIN permissions p ON p.code IN ('algo:view', 'algo:create', 'algo:update', 'algo:enable', 'task:view:all')
    WHERE r.name = 'algorithm_manager';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505000100_AddRbacFoundation') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260505000100_AddRbacFoundation', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081016_AddOperationLogs') THEN

    CREATE TABLE `operation_logs` (
        `id` binary(16) NOT NULL,
        `actor_id` char(16) COLLATE ascii_general_ci NULL,
        `actor_username` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `operation_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `target_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `target_id` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `request_ip` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `request_method` varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `request_path` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `request_summary` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `success` tinyint(1) NOT NULL DEFAULT TRUE,
        `failure_reason` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081016_AddOperationLogs') THEN

    CREATE INDEX `ix_operation_logs_actor_id` ON `operation_logs` (`actor_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081016_AddOperationLogs') THEN

    CREATE INDEX `ix_operation_logs_actor_username` ON `operation_logs` (`actor_username`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081016_AddOperationLogs') THEN

    CREATE INDEX `ix_operation_logs_created_at` ON `operation_logs` (`created_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081016_AddOperationLogs') THEN

    CREATE INDEX `ix_operation_logs_operation_type` ON `operation_logs` (`operation_type`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081016_AddOperationLogs') THEN

    CREATE INDEX `ix_operation_logs_success` ON `operation_logs` (`success`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081016_AddOperationLogs') THEN

    CREATE INDEX `ix_operation_logs_target_type` ON `operation_logs` (`target_type`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081016_AddOperationLogs') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260505081016_AddOperationLogs', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081059_MakeNullableGuidsBinary') THEN

    ALTER TABLE `operation_logs` MODIFY COLUMN `actor_id` binary(16) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081059_MakeNullableGuidsBinary') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260505081059_MakeNullableGuidsBinary', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081730_AddAlgoModels') THEN

    CREATE TABLE `algo_models` (
        `id` int NOT NULL AUTO_INCREMENT,
        `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `enabled` tinyint(1) NOT NULL DEFAULT FALSE,
        `api_url` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `reserved_json` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `deleted_at` datetime NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081730_AddAlgoModels') THEN

    CREATE INDEX `ix_algo_models_created_at` ON `algo_models` (`created_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081730_AddAlgoModels') THEN

    CREATE INDEX `ix_algo_models_enabled` ON `algo_models` (`enabled`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081730_AddAlgoModels') THEN

    CREATE UNIQUE INDEX `ux_algo_models_name` ON `algo_models` (`name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081730_AddAlgoModels') THEN

    INSERT IGNORE INTO algo_models
        (id, name, enabled, api_url, description, reserved_json, created_at, updated_at)
    VALUES
        (1, 'FLDCF', TRUE, 'http://127.0.0.1:8000/detect/fldcf', 'Default FLDCF algorithm endpoint', NULL, UTC_TIMESTAMP(), UTC_TIMESTAMP());

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081730_AddAlgoModels') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260505081730_AddAlgoModels', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081822_EnsureDefaultFldcfAlgoModelId') THEN

    SET @fldcf_generated_id := (
        SELECT id FROM algo_models WHERE name = 'FLDCF' LIMIT 1
    );

    DELETE FROM algo_models
    WHERE id = 1 AND name <> 'FLDCF';

    UPDATE algo_models
    SET id = 1
    WHERE name = 'FLDCF' AND @fldcf_generated_id IS NOT NULL;

    INSERT INTO algo_models
        (id, name, enabled, api_url, description, reserved_json, created_at, updated_at)
    SELECT
        1, 'FLDCF', TRUE, 'http://127.0.0.1:8000/detect/fldcf', 'Default FLDCF algorithm endpoint', NULL, UTC_TIMESTAMP(), UTC_TIMESTAMP()
    WHERE NOT EXISTS (SELECT 1 FROM algo_models WHERE name = 'FLDCF');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505081822_EnsureDefaultFldcfAlgoModelId') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260505081822_EnsureDefaultFldcfAlgoModelId', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505082406_AddAlgoTaskInvocationMetadata') THEN

    ALTER TABLE `algo_task` ADD `algo_api_url` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505082406_AddAlgoTaskInvocationMetadata') THEN

    ALTER TABLE `algo_task` ADD `algo_model_id` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505082406_AddAlgoTaskInvocationMetadata') THEN

    ALTER TABLE `algo_task` ADD `algo_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505082406_AddAlgoTaskInvocationMetadata') THEN

    ALTER TABLE `algo_task` ADD `failure_reason` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505082406_AddAlgoTaskInvocationMetadata') THEN

    CREATE INDEX `ix_algo_task_algo_model_id` ON `algo_task` (`algo_model_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260505082406_AddAlgoTaskInvocationMetadata') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260505082406_AddAlgoTaskInvocationMetadata', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506053729_AddTaskManagementFields') THEN

    ALTER TABLE `task_list` ADD `deleted_at` datetime NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506053729_AddTaskManagementFields') THEN

    ALTER TABLE `algo_task` ADD `deleted_at` datetime NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506053729_AddTaskManagementFields') THEN

    ALTER TABLE `algo_task` ADD `finished_at` datetime NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506053729_AddTaskManagementFields') THEN

    ALTER TABLE `algo_task` ADD `started_at` datetime NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506053729_AddTaskManagementFields') THEN

    CREATE INDEX `ix_task_list_created_at` ON `task_list` (`created_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506053729_AddTaskManagementFields') THEN

    CREATE INDEX `ix_task_list_deleted_at` ON `task_list` (`deleted_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506053729_AddTaskManagementFields') THEN

    CREATE INDEX `ix_task_list_user_id` ON `task_list` (`user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506053729_AddTaskManagementFields') THEN

    CREATE INDEX `ix_algo_task_deleted_at` ON `algo_task` (`deleted_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506053729_AddTaskManagementFields') THEN

    CREATE INDEX `ix_algo_task_finished_at` ON `algo_task` (`finished_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506053729_AddTaskManagementFields') THEN

    CREATE INDEX `ix_algo_task_started_at` ON `algo_task` (`started_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506053729_AddTaskManagementFields') THEN

    CREATE INDEX `ix_algo_task_status` ON `algo_task` (`status`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506053729_AddTaskManagementFields') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260506053729_AddTaskManagementFields', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506054759_AddTaskAudits') THEN

    CREATE TABLE `task_audits` (
        `id` binary(16) NOT NULL,
        `task_id` binary(16) NOT NULL,
        `from_status` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `to_status` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `reason` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `operator_id` binary(16) NULL,
        `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `extra_json` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506054759_AddTaskAudits') THEN

    CREATE INDEX `ix_task_audits_created_at` ON `task_audits` (`created_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506054759_AddTaskAudits') THEN

    CREATE INDEX `ix_task_audits_operator_id` ON `task_audits` (`operator_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506054759_AddTaskAudits') THEN

    CREATE INDEX `ix_task_audits_task_id` ON `task_audits` (`task_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506054759_AddTaskAudits') THEN

    CREATE INDEX `ix_task_audits_to_status` ON `task_audits` (`to_status`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506054759_AddTaskAudits') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260506054759_AddTaskAudits', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506055123_AddTaskAuditForeignKeys') THEN

    ALTER TABLE `task_audits` ADD CONSTRAINT `FK_task_audits_task_list_task_id` FOREIGN KEY (`task_id`) REFERENCES `task_list` (`id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506055123_AddTaskAuditForeignKeys') THEN

    ALTER TABLE `task_audits` ADD CONSTRAINT `FK_task_audits_user_operator_id` FOREIGN KEY (`operator_id`) REFERENCES `user` (`id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506055123_AddTaskAuditForeignKeys') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260506055123_AddTaskAuditForeignKeys', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507112432_AddModelHealthChecks') THEN

    CREATE TABLE `model_health_checks` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `algo_model_id` int NOT NULL,
        `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `response_time_ms` int NULL,
        `checked_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `failure_reason` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`id`),
        CONSTRAINT `FK_model_health_checks_algo_models_algo_model_id` FOREIGN KEY (`algo_model_id`) REFERENCES `algo_models` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507112432_AddModelHealthChecks') THEN

    CREATE INDEX `ix_model_health_checks_algo_model_id` ON `model_health_checks` (`algo_model_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507112432_AddModelHealthChecks') THEN

    CREATE INDEX `ix_model_health_checks_checked_at` ON `model_health_checks` (`checked_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507112432_AddModelHealthChecks') THEN

    CREATE INDEX `ix_model_health_checks_status` ON `model_health_checks` (`status`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507112432_AddModelHealthChecks') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260507112432_AddModelHealthChecks', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507130000_NormalizeRbacTables') THEN

    CREATE TABLE IF NOT EXISTS roles (
        id int NOT NULL AUTO_INCREMENT,
        name varchar(100) NOT NULL,
        description varchar(255) NULL,
        created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        PRIMARY KEY (id),
        UNIQUE KEY ux_roles_name (name)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507130000_NormalizeRbacTables') THEN

    CREATE TABLE IF NOT EXISTS permissions (
        id int NOT NULL AUTO_INCREMENT,
        code varchar(100) NOT NULL,
        description varchar(255) NULL,
        created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
        PRIMARY KEY (id),
        UNIQUE KEY ux_permissions_code (code)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507130000_NormalizeRbacTables') THEN

    CREATE TABLE IF NOT EXISTS user_roles (
        id binary(16) NOT NULL,
        user_id binary(16) NOT NULL,
        role_id int NOT NULL,
        PRIMARY KEY (id),
        UNIQUE KEY ux_user_roles_user_id_role_id (user_id, role_id),
        KEY ix_user_roles_user_id (user_id),
        KEY ix_user_roles_role_id (role_id)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507130000_NormalizeRbacTables') THEN

    CREATE TABLE IF NOT EXISTS role_permissions (
        id binary(16) NOT NULL,
        role_id int NOT NULL,
        permission_id int NOT NULL,
        PRIMARY KEY (id),
        UNIQUE KEY ux_role_permissions_role_id_permission_id (role_id, permission_id),
        KEY ix_role_permissions_role_id (role_id),
        KEY ix_role_permissions_permission_id (permission_id)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507130000_NormalizeRbacTables') THEN

    INSERT IGNORE INTO roles (id, name, description, created_at, updated_at) VALUES
    (1, 'admin', 'System administrator', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (2, 'user', 'Default application user', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (3, 'auditor', 'Audit and task review user', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (4, 'algorithm_manager', 'Algorithm model manager', UTC_TIMESTAMP(), UTC_TIMESTAMP());

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507130000_NormalizeRbacTables') THEN

    INSERT IGNORE INTO permissions (id, code, description, created_at, updated_at) VALUES
    (1, 'task:view:self', 'View own detection tasks', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (2, 'task:view:all', 'View all detection tasks', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (3, 'task:delete', 'Delete detection tasks', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (4, 'task:retry', 'Retry detection tasks', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (5, 'algo:view', 'View algorithm configuration', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (6, 'algo:create', 'Create algorithm configuration', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (7, 'algo:update', 'Update algorithm configuration', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (8, 'algo:enable', 'Enable or disable algorithms', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (9, 'log:view', 'View operation logs', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
    (10, 'admin:access', 'Access administration features', UTC_TIMESTAMP(), UTC_TIMESTAMP());

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507130000_NormalizeRbacTables') THEN

    INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
    SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
    FROM roles r
    JOIN permissions p
    WHERE r.name = 'admin';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507130000_NormalizeRbacTables') THEN

    INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
    SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
    FROM roles r
    JOIN permissions p ON p.code = 'task:view:self'
    WHERE r.name = 'user';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507130000_NormalizeRbacTables') THEN

    INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
    SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
    FROM roles r
    JOIN permissions p ON p.code IN ('task:view:all', 'log:view')
    WHERE r.name = 'auditor';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507130000_NormalizeRbacTables') THEN

    INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
    SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
    FROM roles r
    JOIN permissions p ON p.code IN ('algo:view', 'algo:create', 'algo:update', 'algo:enable', 'task:view:all')
    WHERE r.name = 'algorithm_manager';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507130000_NormalizeRbacTables') THEN

    DROP TABLE IF EXISTS role_permission;
    DROP TABLE IF EXISTS user_role;
    DROP TABLE IF EXISTS permission;
    DROP TABLE IF EXISTS role;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507130000_NormalizeRbacTables') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260507130000_NormalizeRbacTables', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508102743_RemoveLegacyAlgorithmTypeTables') THEN

    DROP TABLE IF EXISTS `task_type_map`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508102743_RemoveLegacyAlgorithmTypeTables') THEN

    DROP TABLE IF EXISTS `algo_type`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508102743_RemoveLegacyAlgorithmTypeTables') THEN


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
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508102743_RemoveLegacyAlgorithmTypeTables') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260508102743_RemoveLegacyAlgorithmTypeTables', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508155710_NormalizeDefaultAlgoModelIds') THEN

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

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508155710_NormalizeDefaultAlgoModelIds') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260508155710_NormalizeDefaultAlgoModelIds', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

