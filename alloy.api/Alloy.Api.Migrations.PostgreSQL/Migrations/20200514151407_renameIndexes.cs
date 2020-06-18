/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.EntityFrameworkCore.Migrations;

namespace Alloy.Api.Migrations.PostgreSQL.Migrations
{
    public partial class renameIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.RenameIndex(
                name: "IX_implementations_definition_id",
                table: "events",
                newName: "IX_events_event_template_id");

            migrationBuilder.DropForeignKey(
                name: "FK_implementations_definitions_definition_id",
                table: "events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_implementations",
                table: "events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_definitions",
                table: "event_templates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_event_templates",
                table: "event_templates",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_events",
                table: "events",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_events_event_templates_event_template_id",
                table: "events",
                column: "event_template_id",
                principalTable: "event_templates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.RenameIndex(
                newName: "IX_implementations_definition_id",
                table: "events",
                name: "IX_events_event_template_id");

            migrationBuilder.DropForeignKey(
                name: "FK_events_event_templates_event_template_id",
                table: "events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_events",
                table: "events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_event_templates",
                table: "event_templates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_definitions",
                table: "event_templates",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_implementations",
                table: "events",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_implementations_definitions_definition_id",
                table: "events",
                column: "event_template_id",
                principalTable: "event_templates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

        }
    }
}
