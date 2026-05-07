using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SIFS.Infrastructure.Database;

#nullable disable

namespace SIFS.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SIFSContext))]
    [Migration("20260507130000_NormalizeRbacTables")]
    public partial class NormalizeRbacTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS roles (
                    id int NOT NULL AUTO_INCREMENT,
                    name varchar(100) NOT NULL,
                    description varchar(255) NULL,
                    created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (id),
                    UNIQUE KEY ux_roles_name (name)
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS permissions (
                    id int NOT NULL AUTO_INCREMENT,
                    code varchar(100) NOT NULL,
                    description varchar(255) NULL,
                    created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (id),
                    UNIQUE KEY ux_permissions_code (code)
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS user_roles (
                    id binary(16) NOT NULL,
                    user_id binary(16) NOT NULL,
                    role_id int NOT NULL,
                    PRIMARY KEY (id),
                    UNIQUE KEY ux_user_roles_user_id_role_id (user_id, role_id),
                    KEY ix_user_roles_user_id (user_id),
                    KEY ix_user_roles_role_id (role_id)
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS role_permissions (
                    id binary(16) NOT NULL,
                    role_id int NOT NULL,
                    permission_id int NOT NULL,
                    PRIMARY KEY (id),
                    UNIQUE KEY ux_role_permissions_role_id_permission_id (role_id, permission_id),
                    KEY ix_role_permissions_role_id (role_id),
                    KEY ix_role_permissions_permission_id (permission_id)
                ) CHARACTER SET=utf8mb4;
                """);

            SeedRbacData(migrationBuilder);

            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS role_permission;
                DROP TABLE IF EXISTS user_role;
                DROP TABLE IF EXISTS permission;
                DROP TABLE IF EXISTS role;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }

        private static void SeedRbacData(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT IGNORE INTO roles (id, name, description, created_at, updated_at) VALUES
                (1, 'admin', 'System administrator', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
                (2, 'user', 'Default application user', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
                (3, 'auditor', 'Audit and task review user', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
                (4, 'algorithm_manager', 'Algorithm model manager', UTC_TIMESTAMP(), UTC_TIMESTAMP());
                """);

            migrationBuilder.Sql("""
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
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
                SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
                FROM roles r
                JOIN permissions p
                WHERE r.name = 'admin';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
                SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
                FROM roles r
                JOIN permissions p ON p.code = 'task:view:self'
                WHERE r.name = 'user';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
                SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
                FROM roles r
                JOIN permissions p ON p.code IN ('task:view:all', 'log:view')
                WHERE r.name = 'auditor';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO role_permissions (id, role_id, permission_id)
                SELECT UNHEX(REPLACE(UUID(), '-', '')), r.id, p.id
                FROM roles r
                JOIN permissions p ON p.code IN ('algo:view', 'algo:create', 'algo:update', 'algo:enable', 'task:view:all')
                WHERE r.name = 'algorithm_manager';
                """);
        }
    }
}
