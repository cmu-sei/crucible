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

namespace Steamfitter.Api.Migrations.PostgreSQL.Migrations
{
    public partial class cascadeDeletes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dispatch_task_results_dispatch_tasks_dispatch_task_id",
                table: "dispatch_task_results");

            migrationBuilder.DropForeignKey(
                name: "FK_dispatch_tasks_scenarios_scenario_id",
                table: "dispatch_tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_dispatch_tasks_sessions_session_id",
                table: "dispatch_tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_dispatch_tasks_dispatch_tasks_trigger_task_id",
                table: "dispatch_tasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "dispatch_task_id",
                table: "dispatch_task_results",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_dispatch_tasks_user_id",
                table: "dispatch_tasks",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_dispatch_task_results_dispatch_tasks_dispatch_task_id",
                table: "dispatch_task_results",
                column: "dispatch_task_id",
                principalTable: "dispatch_tasks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dispatch_tasks_scenarios_scenario_id",
                table: "dispatch_tasks",
                column: "scenario_id",
                principalTable: "scenarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dispatch_tasks_sessions_session_id",
                table: "dispatch_tasks",
                column: "session_id",
                principalTable: "sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dispatch_tasks_dispatch_tasks_trigger_task_id",
                table: "dispatch_tasks",
                column: "trigger_task_id",
                principalTable: "dispatch_tasks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dispatch_task_results_dispatch_tasks_dispatch_task_id",
                table: "dispatch_task_results");

            migrationBuilder.DropForeignKey(
                name: "FK_dispatch_tasks_scenarios_scenario_id",
                table: "dispatch_tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_dispatch_tasks_sessions_session_id",
                table: "dispatch_tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_dispatch_tasks_dispatch_tasks_trigger_task_id",
                table: "dispatch_tasks");

            migrationBuilder.DropIndex(
                name: "IX_dispatch_tasks_user_id",
                table: "dispatch_tasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "dispatch_task_id",
                table: "dispatch_task_results",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_dispatch_task_results_dispatch_tasks_dispatch_task_id",
                table: "dispatch_task_results",
                column: "dispatch_task_id",
                principalTable: "dispatch_tasks",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dispatch_tasks_scenarios_scenario_id",
                table: "dispatch_tasks",
                column: "scenario_id",
                principalTable: "scenarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dispatch_tasks_sessions_session_id",
                table: "dispatch_tasks",
                column: "session_id",
                principalTable: "sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dispatch_tasks_dispatch_tasks_trigger_task_id",
                table: "dispatch_tasks",
                column: "trigger_task_id",
                principalTable: "dispatch_tasks",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
