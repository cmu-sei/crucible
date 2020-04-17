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
    public partial class UserPermissions3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_permission_permissions_permission_id",
                table: "user_permission");

            migrationBuilder.DropForeignKey(
                name: "FK_user_permission_users_user_id",
                table: "user_permission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_permission",
                table: "user_permission");

            migrationBuilder.RenameTable(
                name: "user_permission",
                newName: "user_permissions");

            migrationBuilder.RenameIndex(
                name: "IX_user_permission_user_id_permission_id",
                table: "user_permissions",
                newName: "IX_user_permissions_user_id_permission_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_permission_permission_id",
                table: "user_permissions",
                newName: "IX_user_permissions_permission_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_permissions",
                table: "user_permissions",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_permissions_permissions_permission_id",
                table: "user_permissions",
                column: "permission_id",
                principalTable: "permissions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_permissions_users_user_id",
                table: "user_permissions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_permissions_permissions_permission_id",
                table: "user_permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_user_permissions_users_user_id",
                table: "user_permissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_permissions",
                table: "user_permissions");

            migrationBuilder.RenameTable(
                name: "user_permissions",
                newName: "user_permission");

            migrationBuilder.RenameIndex(
                name: "IX_user_permissions_user_id_permission_id",
                table: "user_permission",
                newName: "IX_user_permission_user_id_permission_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_permissions_permission_id",
                table: "user_permission",
                newName: "IX_user_permission_permission_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_permission",
                table: "user_permission",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_permission_permissions_permission_id",
                table: "user_permission",
                column: "permission_id",
                principalTable: "permissions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_permission_users_user_id",
                table: "user_permission",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

