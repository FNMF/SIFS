-- DEV ONLY: test accounts for local frontend/backend verification.
-- Do not run this script in production deployments.
--
-- Admin account: admin / Admin@123456
-- User account:  user  / User@123456

SET @admin_id = UNHEX(REPLACE('0196ae00-0000-7000-8000-000000000001', '-', ''));
SET @user_id = UNHEX(REPLACE('0196ae00-0000-7000-8000-000000000002', '-', ''));

SET @admin_account = 'admin';
SET @admin_password_hash = 'NoMpGwaOiZmYsNYQW63D6t7ITbjbFf0ZSAiL0uRp5sE=';
SET @admin_salt = 'U0lGUy1BZG1pbi1TZWVkIQ==';

SET @user_account = 'user';
SET @user_password_hash = 'TLb8oLjddU6EsbApGvgXgOsmWC3PErKEz/IgKslWreM=';
SET @user_salt = 'U0lGUy1Vc2VyLS1TZWVkIQ==';

INSERT INTO user (id, account, password_hashed, salt)
SELECT @admin_id, @admin_account, @admin_password_hash, @admin_salt
WHERE NOT EXISTS (SELECT 1 FROM user WHERE account = @admin_account);

UPDATE user
SET password_hashed = @admin_password_hash,
    salt = @admin_salt
WHERE account = @admin_account;

INSERT INTO user (id, account, password_hashed, salt)
SELECT @user_id, @user_account, @user_password_hash, @user_salt
WHERE NOT EXISTS (SELECT 1 FROM user WHERE account = @user_account);

UPDATE user
SET password_hashed = @user_password_hash,
    salt = @user_salt
WHERE account = @user_account;

INSERT IGNORE INTO user_roles (id, user_id, role_id)
SELECT UNHEX(MD5(CONCAT('user_role:', u.account, ':admin'))), u.id, r.id
FROM user u
JOIN roles r ON r.name = 'admin'
WHERE u.account = @admin_account;

INSERT IGNORE INTO user_roles (id, user_id, role_id)
SELECT UNHEX(MD5(CONCAT('user_role:', u.account, ':user'))), u.id, r.id
FROM user u
JOIN roles r ON r.name = 'user'
WHERE u.account = @user_account;
