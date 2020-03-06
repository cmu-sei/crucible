/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Caster.Api.Data.Migrations
{
    public partial class UserPermissionsRemove : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_permission");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_user_id",
                table: "user");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user",
                table: "user");

            migrationBuilder.DropIndex(
                name: "IX_user_id",
                table: "user");

            migrationBuilder.DropColumn(
                name: "key",
                table: "user");

            migrationBuilder.RenameTable(
                name: "user",
                newName: "users");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "users",
                nullable: false,
                defaultValueSql: "uuid_generate_v4()",
                oldClrType: typeof(Guid));

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "user");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "user",
                nullable: false,
                oldClrType: typeof(Guid),
                oldDefaultValueSql: "uuid_generate_v4()");

            migrationBuilder.AddColumn<int>(
                name: "key",
                table: "user",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_user_id",
                table: "user",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user",
                table: "user",
                column: "key");

            migrationBuilder.CreateTable(
                name: "user_permission",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    permission_id = table.Column<Guid>(nullable: false),
                    user_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_permission", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_permission_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_permission_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_id",
                table: "user",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_permission_permission_id",
                table: "user_permission",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_permission_user_id_permission_id",
                table: "user_permission",
                columns: new[] { "user_id", "permission_id" },
                unique: true);
        }
    }
}

