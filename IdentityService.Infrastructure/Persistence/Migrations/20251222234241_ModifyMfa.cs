using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModifyMfa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfile_user_UserId",
                schema: "identity",
                table: "UserProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProfile",
                schema: "identity",
                table: "UserProfile");

            migrationBuilder.RenameTable(
                name: "UserProfile",
                schema: "identity",
                newName: "UserProfiles",
                newSchema: "identity");

            migrationBuilder.RenameIndex(
                name: "IX_UserProfile_UserId",
                schema: "identity",
                table: "UserProfiles",
                newName: "IX_UserProfiles_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProfiles",
                schema: "identity",
                table: "UserProfiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_user_UserId",
                schema: "identity",
                table: "UserProfiles",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_user_UserId",
                schema: "identity",
                table: "UserProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProfiles",
                schema: "identity",
                table: "UserProfiles");

            migrationBuilder.RenameTable(
                name: "UserProfiles",
                schema: "identity",
                newName: "UserProfile",
                newSchema: "identity");

            migrationBuilder.RenameIndex(
                name: "IX_UserProfiles_UserId",
                schema: "identity",
                table: "UserProfile",
                newName: "IX_UserProfile_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProfile",
                schema: "identity",
                table: "UserProfile",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfile_user_UserId",
                schema: "identity",
                table: "UserProfile",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
