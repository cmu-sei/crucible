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

namespace Steamfitter.Api.Migrations.PostgreSQL.Migrations
{
    public partial class vmcreds2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vm_credential_entity_scenarios_scenario_id",
                table: "vm_credential_entity");

            migrationBuilder.DropForeignKey(
                name: "FK_vm_credential_entity_scenario_templates_scenario_template_id",
                table: "vm_credential_entity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vm_credential_entity",
                table: "vm_credential_entity");

            migrationBuilder.RenameTable(
                name: "vm_credential_entity",
                newName: "vm_credentials");

            migrationBuilder.RenameIndex(
                name: "IX_vm_credential_entity_scenario_template_id",
                table: "vm_credentials",
                newName: "IX_vm_credentials_scenario_template_id");

            migrationBuilder.RenameIndex(
                name: "IX_vm_credential_entity_scenario_id",
                table: "vm_credentials",
                newName: "IX_vm_credentials_scenario_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vm_credentials",
                table: "vm_credentials",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_vm_credentials_scenarios_scenario_id",
                table: "vm_credentials",
                column: "scenario_id",
                principalTable: "scenarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vm_credentials_scenario_templates_scenario_template_id",
                table: "vm_credentials",
                column: "scenario_template_id",
                principalTable: "scenario_templates",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vm_credentials_scenarios_scenario_id",
                table: "vm_credentials");

            migrationBuilder.DropForeignKey(
                name: "FK_vm_credentials_scenario_templates_scenario_template_id",
                table: "vm_credentials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vm_credentials",
                table: "vm_credentials");

            migrationBuilder.RenameTable(
                name: "vm_credentials",
                newName: "vm_credential_entity");

            migrationBuilder.RenameIndex(
                name: "IX_vm_credentials_scenario_template_id",
                table: "vm_credential_entity",
                newName: "IX_vm_credential_entity_scenario_template_id");

            migrationBuilder.RenameIndex(
                name: "IX_vm_credentials_scenario_id",
                table: "vm_credential_entity",
                newName: "IX_vm_credential_entity_scenario_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vm_credential_entity",
                table: "vm_credential_entity",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_vm_credential_entity_scenarios_scenario_id",
                table: "vm_credential_entity",
                column: "scenario_id",
                principalTable: "scenarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vm_credential_entity_scenario_templates_scenario_template_id",
                table: "vm_credential_entity",
                column: "scenario_template_id",
                principalTable: "scenario_templates",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
