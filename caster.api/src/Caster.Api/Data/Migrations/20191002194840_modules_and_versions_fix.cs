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

namespace Caster.Api.Data.Migrations
{
    public partial class modules_and_versions_fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_version_module_module_id",
                table: "version");

            migrationBuilder.DropPrimaryKey(
                name: "PK_version",
                table: "version");

            migrationBuilder.DropPrimaryKey(
                name: "PK_module",
                table: "module");

            migrationBuilder.RenameTable(
                name: "version",
                newName: "versions");

            migrationBuilder.RenameTable(
                name: "module",
                newName: "modules");

            migrationBuilder.RenameIndex(
                name: "IX_version_module_id",
                table: "versions",
                newName: "IX_versions_module_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_versions",
                table: "versions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_modules",
                table: "modules",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_versions_modules_module_id",
                table: "versions",
                column: "module_id",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_versions_modules_module_id",
                table: "versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_versions",
                table: "versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_modules",
                table: "modules");

            migrationBuilder.RenameTable(
                name: "versions",
                newName: "version");

            migrationBuilder.RenameTable(
                name: "modules",
                newName: "module");

            migrationBuilder.RenameIndex(
                name: "IX_versions_module_id",
                table: "version",
                newName: "IX_version_module_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_version",
                table: "version",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_module",
                table: "module",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_version_module_module_id",
                table: "version",
                column: "module_id",
                principalTable: "module",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

