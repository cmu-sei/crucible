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

namespace S3.Player.Api.Migrations.PostgreSQL.Migrations
{
    public partial class Fixed_Membership_Relation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exercise_memberships_team_memberships_primary_team_membersh~",
                table: "exercise_memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_team_memberships_exercise_memberships_exercise_membership_e~",
                table: "team_memberships");

            migrationBuilder.DropIndex(
                name: "IX_team_memberships_exercise_membership_entity_id",
                table: "team_memberships");

            migrationBuilder.DropIndex(
                name: "IX_exercise_memberships_primary_team_membership_id",
                table: "exercise_memberships");

            migrationBuilder.DropColumn(
                name: "exercise_membership_entity_id",
                table: "team_memberships");

            migrationBuilder.AlterColumn<Guid>(
                name: "primary_team_membership_id",
                table: "exercise_memberships",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.CreateIndex(
                name: "IX_team_memberships_exercise_membership_id",
                table: "team_memberships",
                column: "exercise_membership_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_memberships_primary_team_membership_id",
                table: "exercise_memberships",
                column: "primary_team_membership_id");

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_memberships_team_memberships_primary_team_membersh~",
                table: "exercise_memberships",
                column: "primary_team_membership_id",
                principalTable: "team_memberships",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_team_memberships_exercise_memberships_exercise_membership_id",
                table: "team_memberships",
                column: "exercise_membership_id",
                principalTable: "exercise_memberships",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exercise_memberships_team_memberships_primary_team_membersh~",
                table: "exercise_memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_team_memberships_exercise_memberships_exercise_membership_id",
                table: "team_memberships");

            migrationBuilder.DropIndex(
                name: "IX_team_memberships_exercise_membership_id",
                table: "team_memberships");

            migrationBuilder.DropIndex(
                name: "IX_exercise_memberships_primary_team_membership_id",
                table: "exercise_memberships");

            migrationBuilder.AddColumn<Guid>(
                name: "exercise_membership_entity_id",
                table: "team_memberships",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "primary_team_membership_id",
                table: "exercise_memberships",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_team_memberships_exercise_membership_entity_id",
                table: "team_memberships",
                column: "exercise_membership_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_memberships_primary_team_membership_id",
                table: "exercise_memberships",
                column: "primary_team_membership_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_memberships_team_memberships_primary_team_membersh~",
                table: "exercise_memberships",
                column: "primary_team_membership_id",
                principalTable: "team_memberships",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_team_memberships_exercise_memberships_exercise_membership_e~",
                table: "team_memberships",
                column: "exercise_membership_entity_id",
                principalTable: "exercise_memberships",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

