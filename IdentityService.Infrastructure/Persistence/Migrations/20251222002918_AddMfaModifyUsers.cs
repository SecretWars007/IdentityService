using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMfaModifyUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MfaEnabled",
                schema: "identity",
                table: "user");

            migrationBuilder.DropColumn(
                name: "MfaSecret",
                schema: "identity",
                table: "user");

            migrationBuilder.CreateTable(
                name: "user_mfa",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Secret = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_mfa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_mfa_user_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_mfa_UserId",
                schema: "identity",
                table: "user_mfa",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_mfa",
                schema: "identity");

            migrationBuilder.AddColumn<bool>(
                name: "MfaEnabled",
                schema: "identity",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MfaSecret",
                schema: "identity",
                table: "user",
                type: "text",
                nullable: true);
        }
    }
}
