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

namespace Foreman.Api.Migrations.PostgreSQL.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "webhooks",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    status = table.Column<int>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    postback_url = table.Column<string>(nullable: true),
                    postback_method = table.Column<int>(nullable: false),
                    must_authenticate = table.Column<bool>(nullable: false),
                    payload = table.Column<string>(nullable: true),
                    created_utc = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhooks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workorders",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    group_name = table.Column<string>(nullable: true),
                    start = table.Column<DateTime>(nullable: false),
                    end = table.Column<DateTime>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    job = table.Column<int>(nullable: false),
                    webhook_id = table.Column<Guid>(nullable: false),
                    job_key = table.Column<int>(nullable: true),
                    created_utc = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workorders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workorderparameters",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(nullable: true),
                    value = table.Column<string>(nullable: true),
                    work_order_id = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workorderparameters", x => x.id);
                    table.ForeignKey(
                        name: "FK_workorderparameters_workorders_work_order_id",
                        column: x => x.work_order_id,
                        principalTable: "workorders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workorderresults",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_utc = table.Column<DateTime>(nullable: false),
                    payload = table.Column<string>(nullable: true),
                    work_order_id = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workorderresults", x => x.id);
                    table.ForeignKey(
                        name: "FK_workorderresults_workorders_work_order_id",
                        column: x => x.work_order_id,
                        principalTable: "workorders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workordertriggers",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    group_name = table.Column<string>(nullable: true),
                    interval = table.Column<int>(nullable: false),
                    work_order_id = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workordertriggers", x => x.id);
                    table.ForeignKey(
                        name: "FK_workordertriggers_workorders_work_order_id",
                        column: x => x.work_order_id,
                        principalTable: "workorders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workorderparameters_work_order_id",
                table: "workorderparameters",
                column: "work_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_workorderresults_work_order_id",
                table: "workorderresults",
                column: "work_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_workordertriggers_work_order_id",
                table: "workordertriggers",
                column: "work_order_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "webhooks");

            migrationBuilder.DropTable(
                name: "workorderparameters");

            migrationBuilder.DropTable(
                name: "workorderresults");

            migrationBuilder.DropTable(
                name: "workordertriggers");

            migrationBuilder.DropTable(
                name: "workorders");
        }
    }
}

