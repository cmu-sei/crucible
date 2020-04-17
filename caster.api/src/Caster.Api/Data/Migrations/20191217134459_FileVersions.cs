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
    public partial class FileVersions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_saved",
                table: "files",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "files",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "files",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "file_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    file_id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(nullable: true),
                    user_id = table.Column<Guid>(nullable: true),
                    content = table.Column<string>(nullable: true),
                    date_saved = table.Column<DateTime>(nullable: true),
                    tag = table.Column<string>(nullable: true),
                    date_tagged = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_file_versions_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_file_versions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_files_user_id",
                table: "files",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_file_versions_file_id",
                table: "file_versions",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "IX_file_versions_user_id",
                table: "file_versions",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_files_users_user_id",
                table: "files",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_files_users_user_id",
                table: "files");

            migrationBuilder.DropTable(
                name: "file_versions");

            migrationBuilder.DropIndex(
                name: "IX_files_user_id",
                table: "files");

            migrationBuilder.DropColumn(
                name: "date_saved",
                table: "files");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "files");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "files");
        }
    }
}

