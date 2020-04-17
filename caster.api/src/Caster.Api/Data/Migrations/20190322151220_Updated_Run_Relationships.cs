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
    public partial class Updated_Run_Relationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_runs_applies_apply_id",
                table: "runs");

            migrationBuilder.DropForeignKey(
                name: "FK_runs_plans_plan_id",
                table: "runs");

            migrationBuilder.DropIndex(
                name: "IX_runs_apply_id",
                table: "runs");

            migrationBuilder.DropIndex(
                name: "IX_runs_plan_id",
                table: "runs");

            migrationBuilder.DropColumn(
                name: "apply_id",
                table: "runs");

            migrationBuilder.DropColumn(
                name: "plan_id",
                table: "runs");

            migrationBuilder.CreateIndex(
                name: "IX_plans_run_id",
                table: "plans",
                column: "run_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_applies_run_id",
                table: "applies",
                column: "run_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_applies_runs_run_id",
                table: "applies",
                column: "run_id",
                principalTable: "runs",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_plans_runs_run_id",
                table: "plans",
                column: "run_id",
                principalTable: "runs",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_applies_runs_run_id",
                table: "applies");

            migrationBuilder.DropForeignKey(
                name: "FK_plans_runs_run_id",
                table: "plans");

            migrationBuilder.DropIndex(
                name: "IX_plans_run_id",
                table: "plans");

            migrationBuilder.DropIndex(
                name: "IX_applies_run_id",
                table: "applies");

            migrationBuilder.AddColumn<Guid>(
                name: "apply_id",
                table: "runs",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "plan_id",
                table: "runs",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_runs_apply_id",
                table: "runs",
                column: "apply_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_runs_plan_id",
                table: "runs",
                column: "plan_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_runs_applies_apply_id",
                table: "runs",
                column: "apply_id",
                principalTable: "applies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_runs_plans_plan_id",
                table: "runs",
                column: "plan_id",
                principalTable: "plans",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

