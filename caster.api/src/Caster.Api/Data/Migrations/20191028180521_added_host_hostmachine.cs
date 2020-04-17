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

namespace Caster.Api.Data.Migrations
{
    public partial class added_host_hostmachine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "host_id",
                table: "workspaces",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "dynamic_hosts",
                table: "exercises",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "hosts",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(nullable: true),
                    datastore = table.Column<string>(nullable: true),
                    maximum_machines = table.Column<int>(nullable: false),
                    exercise_id = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hosts", x => x.id);
                    table.ForeignKey(
                        name: "FK_hosts_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "host_machines",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(nullable: true),
                    workspace_id = table.Column<Guid>(nullable: false),
                    host_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_host_machines", x => x.id);
                    table.ForeignKey(
                        name: "FK_host_machines_hosts_host_id",
                        column: x => x.host_id,
                        principalTable: "hosts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_host_machines_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_host_id",
                table: "workspaces",
                column: "host_id");

            migrationBuilder.CreateIndex(
                name: "IX_host_machines_host_id",
                table: "host_machines",
                column: "host_id");

            migrationBuilder.CreateIndex(
                name: "IX_host_machines_workspace_id",
                table: "host_machines",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_hosts_exercise_id",
                table: "hosts",
                column: "exercise_id");

            migrationBuilder.AddForeignKey(
                name: "FK_workspaces_hosts_host_id",
                table: "workspaces",
                column: "host_id",
                principalTable: "hosts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_workspaces_hosts_host_id",
                table: "workspaces");

            migrationBuilder.DropTable(
                name: "host_machines");

            migrationBuilder.DropTable(
                name: "hosts");

            migrationBuilder.DropIndex(
                name: "IX_workspaces_host_id",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "host_id",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "dynamic_hosts",
                table: "exercises");
        }
    }
}

