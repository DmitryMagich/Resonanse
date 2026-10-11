using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resonanse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestRoleAndInviteRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTestAlbum",
                table: "Albums",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "InviteRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    Contact = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InviteRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InviteRequests_Users_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_IsTestAlbum",
                table: "Albums",
                column: "IsTestAlbum");

            migrationBuilder.CreateIndex(
                name: "IX_InviteRequests_CreatedAt",
                table: "InviteRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InviteRequests_ProcessedByUserId",
                table: "InviteRequests",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InviteRequests_Status",
                table: "InviteRequests",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InviteRequests");

            migrationBuilder.DropIndex(
                name: "IX_Albums_IsTestAlbum",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "IsTestAlbum",
                table: "Albums");
        }
    }
}
