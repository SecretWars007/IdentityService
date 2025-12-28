using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TokenVersion",
                schema: "identity",
                table: "user",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                schema: "identity",
                table: "SystemSettings",
                columns: new[] { "Id", "LockoutMinutes", "MaxFailedLoginAttempts", "MaxLockouts" },
                values: new object[] { new Guid("2e78f358-cd37-44b0-8967-02aed1ea2d5a"), 15, 3, 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("2e78f358-cd37-44b0-8967-02aed1ea2d5a"));

            migrationBuilder.DropColumn(
                name: "TokenVersion",
                schema: "identity",
                table: "user");
        }
    }
}
