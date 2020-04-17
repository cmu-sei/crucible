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
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "files",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    date_created = table.Column<DateTime>(nullable: false),
                    date_modified = table.Column<DateTime>(nullable: true),
                    created_by = table.Column<Guid>(nullable: false),
                    modified_by = table.Column<Guid>(nullable: true),
                    name = table.Column<string>(nullable: true),
                    storage_path = table.Column<string>(nullable: true),
                    length = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scenarios",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    date_created = table.Column<DateTime>(nullable: false),
                    date_modified = table.Column<DateTime>(nullable: true),
                    created_by = table.Column<Guid>(nullable: false),
                    modified_by = table.Column<Guid>(nullable: true),
                    name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    duration_hours = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scenarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    key = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    date_created = table.Column<DateTime>(nullable: false),
                    date_modified = table.Column<DateTime>(nullable: true),
                    created_by = table.Column<Guid>(nullable: false),
                    modified_by = table.Column<Guid>(nullable: true),
                    id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    date_created = table.Column<DateTime>(nullable: false),
                    date_modified = table.Column<DateTime>(nullable: true),
                    created_by = table.Column<Guid>(nullable: false),
                    modified_by = table.Column<Guid>(nullable: true),
                    name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    start_date = table.Column<DateTime>(nullable: false),
                    end_date = table.Column<DateTime>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    on_demand = table.Column<bool>(nullable: false),
                    scenario_id = table.Column<Guid>(nullable: true),
                    exercise_id = table.Column<Guid>(nullable: true),
                    exercise = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_sessions_scenarios_scenario_id",
                        column: x => x.scenario_id,
                        principalTable: "scenarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dispatch_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    date_created = table.Column<DateTime>(nullable: false),
                    date_modified = table.Column<DateTime>(nullable: true),
                    created_by = table.Column<Guid>(nullable: false),
                    modified_by = table.Column<Guid>(nullable: true),
                    name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    scenario_id = table.Column<Guid>(nullable: true),
                    session_id = table.Column<Guid>(nullable: true),
                    user_id = table.Column<Guid>(nullable: true),
                    action = table.Column<int>(nullable: false),
                    vm_mask = table.Column<string>(nullable: true),
                    api_url = table.Column<string>(nullable: true),
                    input_string = table.Column<string>(nullable: true),
                    expected_output = table.Column<string>(nullable: true),
                    expiration_seconds = table.Column<int>(nullable: false),
                    delay_seconds = table.Column<int>(nullable: false),
                    interval_seconds = table.Column<int>(nullable: false),
                    iterations = table.Column<int>(nullable: false),
                    trigger_task_id = table.Column<Guid>(nullable: true),
                    trigger_condition = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispatch_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_dispatch_tasks_scenarios_scenario_id",
                        column: x => x.scenario_id,
                        principalTable: "scenarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dispatch_tasks_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dispatch_tasks_dispatch_tasks_trigger_task_id",
                        column: x => x.trigger_task_id,
                        principalTable: "dispatch_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dispatch_task_results",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    date_created = table.Column<DateTime>(nullable: false),
                    date_modified = table.Column<DateTime>(nullable: true),
                    created_by = table.Column<Guid>(nullable: false),
                    modified_by = table.Column<Guid>(nullable: true),
                    dispatch_task_id = table.Column<Guid>(nullable: true),
                    vm_id = table.Column<Guid>(nullable: true),
                    vm_name = table.Column<string>(nullable: true),
                    api_url = table.Column<string>(nullable: true),
                    input_string = table.Column<string>(nullable: true),
                    expiration_seconds = table.Column<int>(nullable: false),
                    iterations = table.Column<int>(nullable: false),
                    interval_seconds = table.Column<int>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    expected_output = table.Column<string>(nullable: true),
                    actual_output = table.Column<string>(nullable: true),
                    sent_date = table.Column<DateTime>(nullable: false),
                    status_date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispatch_task_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_dispatch_task_results_dispatch_tasks_dispatch_task_id",
                        column: x => x.dispatch_task_id,
                        principalTable: "dispatch_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dispatch_task_results_dispatch_task_id",
                table: "dispatch_task_results",
                column: "dispatch_task_id");

            migrationBuilder.CreateIndex(
                name: "IX_dispatch_tasks_scenario_id",
                table: "dispatch_tasks",
                column: "scenario_id");

            migrationBuilder.CreateIndex(
                name: "IX_dispatch_tasks_session_id",
                table: "dispatch_tasks",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_dispatch_tasks_trigger_task_id",
                table: "dispatch_tasks",
                column: "trigger_task_id");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_scenario_id",
                table: "sessions",
                column: "scenario_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_id",
                table: "users",
                column: "id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dispatch_task_results");

            migrationBuilder.DropTable(
                name: "files");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "dispatch_tasks");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "scenarios");
        }
    }
}

