/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Steamfitter.Api.Migrations.PostgreSQL.Migrations
{
    public partial class Permissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropColumn(
                name: "key",
                table: "users");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "users",
                nullable: false,
                defaultValueSql: "uuid_generate_v4()",
                oldClrType: typeof(Guid));

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "id");

            migrationBuilder.CreateTable(
                name: "os",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    platform = table.Column<string>(nullable: true),
                    service_pack = table.Column<string>(nullable: true),
                    version = table.Column<string>(nullable: true),
                    version_string = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    key = table.Column<string>(nullable: true),
                    value = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    read_only = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exercise_agents",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    machine_name = table.Column<string>(nullable: true),
                    fqdn = table.Column<string>(nullable: true),
                    guest_ip = table.Column<string>(nullable: true),
                    vm_ware_uuid = table.Column<Guid>(nullable: false),
                    vm_ware_name = table.Column<Guid>(nullable: false),
                    agent_name = table.Column<string>(nullable: true),
                    agent_version = table.Column<string>(nullable: true),
                    agent_installed_path = table.Column<string>(nullable: true),
                    operating_system_id = table.Column<int>(nullable: true),
                    checkin_time = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_agents", x => x.id);
                    table.ForeignKey(
                        name: "FK_exercise_agents_os_operating_system_id",
                        column: x => x.operating_system_id,
                        principalTable: "os",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(nullable: false),
                    permission_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_permissions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "local_user",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    username = table.Column<string>(nullable: true),
                    domain = table.Column<string>(nullable: true),
                    is_current = table.Column<bool>(nullable: false),
                    exercise_agent_id = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_user", x => x.id);
                    table.ForeignKey(
                        name: "FK_local_user_exercise_agents_exercise_agent_id",
                        column: x => x.exercise_agent_id,
                        principalTable: "exercise_agents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "monitored_tool",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(nullable: true),
                    is_running = table.Column<bool>(nullable: false),
                    version = table.Column<string>(nullable: true),
                    location = table.Column<string>(nullable: true),
                    exercise_agent_id = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monitored_tool", x => x.id);
                    table.ForeignKey(
                        name: "FK_monitored_tool_exercise_agents_exercise_agent_id",
                        column: x => x.exercise_agent_id,
                        principalTable: "exercise_agents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ssh_port",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    server = table.Column<string>(nullable: true),
                    server_port = table.Column<long>(nullable: false),
                    guest = table.Column<string>(nullable: true),
                    guest_port = table.Column<long>(nullable: false),
                    exercise_agent_id = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ssh_port", x => x.id);
                    table.ForeignKey(
                        name: "FK_ssh_port_exercise_agents_exercise_agent_id",
                        column: x => x.exercise_agent_id,
                        principalTable: "exercise_agents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exercise_agents_operating_system_id",
                table: "exercise_agents",
                column: "operating_system_id");

            migrationBuilder.CreateIndex(
                name: "IX_local_user_exercise_agent_id",
                table: "local_user",
                column: "exercise_agent_id");

            migrationBuilder.CreateIndex(
                name: "IX_monitored_tool_exercise_agent_id",
                table: "monitored_tool",
                column: "exercise_agent_id");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_key_value",
                table: "permissions",
                columns: new[] { "key", "value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ssh_port_exercise_agent_id",
                table: "ssh_port",
                column: "exercise_agent_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_permissions_permission_id",
                table: "user_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_permissions_user_id_permission_id",
                table: "user_permissions",
                columns: new[] { "user_id", "permission_id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "local_user");

            migrationBuilder.DropTable(
                name: "monitored_tool");

            migrationBuilder.DropTable(
                name: "ssh_port");

            migrationBuilder.DropTable(
                name: "user_permissions");

            migrationBuilder.DropTable(
                name: "exercise_agents");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "os");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "users",
                nullable: false,
                oldClrType: typeof(Guid),
                oldDefaultValueSql: "uuid_generate_v4()");

            migrationBuilder.AddColumn<int>(
                name: "key",
                table: "users",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "key");
        }
    }
}

