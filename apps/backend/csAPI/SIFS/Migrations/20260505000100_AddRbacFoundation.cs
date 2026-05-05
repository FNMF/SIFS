using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SIFS.Infrastructure.Database;

#nullable disable

namespace SIFS.Migrations
{
    [DbContext(typeof(SIFSContext))]
    [Migration("20260505000100_AddRbacFoundation")]
    public partial class AddRbacFoundation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<byte[]>(type: "binary(16)", fixedLength: true, maxLength: 16, nullable: false),
                    user_id = table.Column<byte[]>(type: "binary(16)", fixedLength: true, maxLength: 16, nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<byte[]>(type: "binary(16)", fixedLength: true, maxLength: 16, nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    permission_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_permissions_code",
                table: "permissions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id",
                table: "user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ux_user_roles_user_id_role_id",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_id",
                table: "role_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ux_role_permissions_role_id_permission_id",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_id" },
                unique: true);

            SeedRbacData(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "role_permissions");
            migrationBuilder.DropTable(name: "user_roles");
            migrationBuilder.DropTable(name: "permissions");
            migrationBuilder.DropTable(name: "roles");
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
