-- RBAC foundation seed data.
-- Safe to run repeatedly.

INSERT INTO roles (id, name, description, created_at, updated_at) VALUES
(1, 'admin', 'System administrator', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(2, 'user', 'Default application user', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(3, 'auditor', 'Audit and task review user', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(4, 'algorithm_manager', 'Algorithm model manager', UTC_TIMESTAMP(), UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE
    name = VALUES(name),
    description = VALUES(description),
    updated_at = UTC_TIMESTAMP();

INSERT INTO permissions (id, code, description, created_at, updated_at) VALUES
(1, 'task:view:self', 'View own detection tasks', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(2, 'task:view:all', 'View all detection tasks', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(3, 'task:delete', 'Delete detection tasks', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(4, 'task:retry', 'Retry detection tasks', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(5, 'algo:view', 'View algorithm configuration', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(6, 'algo:create', 'Create algorithm configuration', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(7, 'algo:update', 'Update algorithm configuration', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(8, 'algo:enable', 'Enable or disable algorithms', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(9, 'log:view', 'View operation logs', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
(10, 'admin:access', 'Access administration features', UTC_TIMESTAMP(), UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE
    code = VALUES(code),
    description = VALUES(description),
    updated_at = UTC_TIMESTAMP();

INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
SELECT UNHEX(MD5(CONCAT('role_permission:', r.name, ':', p.code))), r.id, p.id
FROM roles r
JOIN permissions p
WHERE r.name = 'admin';

INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
SELECT UNHEX(MD5(CONCAT('role_permission:', r.name, ':', p.code))), r.id, p.id
FROM roles r
JOIN permissions p ON p.code = 'task:view:self'
WHERE r.name = 'user';

INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
SELECT UNHEX(MD5(CONCAT('role_permission:', r.name, ':', p.code))), r.id, p.id
FROM roles r
JOIN permissions p ON p.code IN ('task:view:all', 'log:view')
WHERE r.name = 'auditor';

INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
SELECT UNHEX(MD5(CONCAT('role_permission:', r.name, ':', p.code))), r.id, p.id
FROM roles r
JOIN permissions p ON p.code IN ('algo:view', 'algo:create', 'algo:update', 'algo:enable', 'task:view:all')
WHERE r.name = 'algorithm_manager';
