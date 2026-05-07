-- Test admin seed account.
-- Account: admin
-- Password: Admin@123456
--
-- This script is idempotent. It creates or updates the test admin account
-- and grants the admin role to every user row whose account is "admin".

SET @admin_account = 'admin';
SET @admin_password_hash = 'NoMpGwaOiZmYsNYQW63D6t7ITbjbFf0ZSAiL0uRp5sE=';
SET @admin_salt = 'U0lGUy1BZG1pbi1TZWVkIQ==';

INSERT IGNORE INTO roles (id, name, description, created_at, updated_at) VALUES
(1, 'admin', 'System administrator', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(2, 'user', 'Default application user', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(3, 'auditor', 'Audit and task review user', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(4, 'algorithm_manager', 'Algorithm model manager', UTC_TIMESTAMP(), UTC_TIMESTAMP());

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

INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
FROM roles r
JOIN permissions p
WHERE r.name = 'admin';

INSERT INTO user (id, account, password_hashed, salt)
SELECT UNHEX(REPLACE('0196ae00-0000-7000-8000-000000000001', '-', '')), @admin_account, @admin_password_hash, @admin_salt
WHERE NOT EXISTS (
    SELECT 1 FROM user WHERE account = @admin_account
);

UPDATE user
SET password_hashed = @admin_password_hash,
    salt = @admin_salt
WHERE account = @admin_account;

INSERT IGNORE INTO user_roles (id, user_id, role_id)
SELECT UNHEX(REPLACE(UUID(), '-', '')), u.id, r.id
FROM user u
JOIN roles r ON r.name = 'admin'
WHERE u.account = @admin_account;
