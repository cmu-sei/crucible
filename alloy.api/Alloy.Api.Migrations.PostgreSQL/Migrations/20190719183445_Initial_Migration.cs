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

namespace Alloy.Api.Migrations.PostgreSQL.Migrations
{
    public partial class Initial_Migration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    date_created = table.Column<DateTime>(nullable: false),
                    date_modified = table.Column<DateTime>(nullable: true),
                    created_by = table.Column<Guid>(nullable: false),
                    modified_by = table.Column<Guid>(nullable: true),
                    exercise_id = table.Column<Guid>(nullable: true),
                    directory_id = table.Column<Guid>(nullable: true),
                    scenario_id = table.Column<Guid>(nullable: true),
                    name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    duration_hours = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "implementations",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    date_created = table.Column<DateTime>(nullable: false),
                    date_modified = table.Column<DateTime>(nullable: true),
                    created_by = table.Column<Guid>(nullable: false),
                    modified_by = table.Column<Guid>(nullable: true),
                    user_id = table.Column<Guid>(nullable: false),
                    username = table.Column<string>(nullable: true),
                    definition_id = table.Column<Guid>(nullable: true),
                    exercise_id = table.Column<Guid>(nullable: true),
                    workspace_id = table.Column<Guid>(nullable: true),
                    run_id = table.Column<Guid>(nullable: true),
                    session_id = table.Column<Guid>(nullable: true),
                    name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    status = table.Column<int>(nullable: false),
                    launch_date = table.Column<DateTime>(nullable: true),
                    end_date = table.Column<DateTime>(nullable: true),
                    expiration_date = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_implementations", x => x.id);
                    table.ForeignKey(
                        name: "FK_implementations_definitions_definition_id",
                        column: x => x.definition_id,
                        principalTable: "definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_implementations_definition_id",
                table: "implementations",
                column: "definition_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "implementations");

            migrationBuilder.DropTable(
                name: "definitions");
        }
    }
}

