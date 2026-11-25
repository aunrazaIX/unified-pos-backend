using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifiedPOS.Infrastructure.Databases.Migrations
{
    /// <inheritdoc />
    public partial class UnifiedPasswordAndRegisterAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_OrganizationId_NormalizedUserName",
                table: "User");

            migrationBuilder.CreateTable(
                name: "UserRegister",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegisterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRegister", x => new { x.UserId, x.RegisterId });
                    table.ForeignKey(
                        name: "FK_UserRegister_Register_RegisterId",
                        column: x => x.RegisterId,
                        principalTable: "Register",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRegister_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_NormalizedUserName",
                table: "User",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_OrganizationId",
                table: "User",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRegister_RegisterId",
                table: "UserRegister",
                column: "RegisterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRegister");

            migrationBuilder.DropIndex(
                name: "IX_User_NormalizedUserName",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_OrganizationId",
                table: "User");

            migrationBuilder.CreateIndex(
                name: "IX_User_OrganizationId_NormalizedUserName",
                table: "User",
                columns: new[] { "OrganizationId", "NormalizedUserName" },
                unique: true);
        }
    }
}
