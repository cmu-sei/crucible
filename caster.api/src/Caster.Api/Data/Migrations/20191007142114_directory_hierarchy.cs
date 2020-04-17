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
    public partial class directory_hierarchy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_files_workspaces_workspace_id",
                table: "files");

            migrationBuilder.DropColumn(
                name: "shared",
                table: "directories");

            migrationBuilder.AddColumn<Guid>(
                name: "parent_id",
                table: "directories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "path",
                table: "directories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_directories_parent_id",
                table: "directories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_directories_path",
                table: "directories",
                column: "path");

            migrationBuilder.AddForeignKey(
                name: "FK_directories_directories_parent_id",
                table: "directories",
                column: "parent_id",
                principalTable: "directories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_files_workspaces_workspace_id",
                table: "files",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_directories_directories_parent_id",
                table: "directories");

            migrationBuilder.DropForeignKey(
                name: "FK_files_workspaces_workspace_id",
                table: "files");

            migrationBuilder.DropIndex(
                name: "IX_directories_parent_id",
                table: "directories");

            migrationBuilder.DropIndex(
                name: "IX_directories_path",
                table: "directories");

            migrationBuilder.DropColumn(
                name: "parent_id",
                table: "directories");

            migrationBuilder.DropColumn(
                name: "path",
                table: "directories");

            migrationBuilder.AddColumn<bool>(
                name: "shared",
                table: "directories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_files_workspaces_workspace_id",
                table: "files",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

