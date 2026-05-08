-- Baseline schema for legacy tables that existed before EF migrations were introduced.
-- Run this before `dotnet ef database update` when bootstrapping an empty database.

CREATE TABLE IF NOT EXISTS `user` (
    `id` binary(16) NOT NULL,
    `password_hashed` varchar(255) NOT NULL,
    `salt` varchar(255) NOT NULL,
    `account` varchar(255) NOT NULL,
    PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `task_list` (
    `id` binary(16) NOT NULL,
    `status` int NOT NULL,
    `user_id` binary(16) NOT NULL,
    `created_at` datetime NOT NULL,
    `updated_at` datetime NOT NULL,
    `level` int NULL,
    PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `algo_task` (
    `id` binary(16) NOT NULL,
    `task_id` binary(16) NOT NULL,
    `created_at` datetime NOT NULL,
    `updated_at` datetime NOT NULL,
    `status` int NOT NULL,
    PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `localfile` (
    `id` binary(16) NOT NULL,
    `url_local` varchar(255) NOT NULL,
    `url_cloud` varchar(255) NULL,
    `algo_task_id` binary(16) NOT NULL,
    `sid` int NOT NULL,
    `created_at` datetime NOT NULL,
    `updated_at` datetime NOT NULL,
    PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `result_file` (
    `id` binary(16) NOT NULL,
    `algo_task_id` binary(16) NOT NULL,
    `is_fake` tinyint(1) NULL,
    `confidence` double NULL,
    `mask_local_url` varchar(255) NULL,
    `mask_cloud_url` varchar(255) NULL,
    PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `refresh_token` (
    `id` binary(16) NOT NULL,
    `user_id` binary(16) NOT NULL,
    `token` varchar(255) NOT NULL,
    `expires_at` datetime NOT NULL,
    `is_revoked` tinyint(1) NOT NULL,
    PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
