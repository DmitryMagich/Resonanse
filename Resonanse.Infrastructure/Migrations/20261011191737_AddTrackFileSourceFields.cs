using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resonanse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackFileSourceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "TrackFiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "TrackFiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalMetadataJson",
                table: "TrackFiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinnedByUser",
                table: "TrackFiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "TrackFiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrackFiles_ExpiresAt",
                table: "TrackFiles",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrackFiles_Source",
                table: "TrackFiles",
                column: "Source");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrackFiles_ExpiresAt",
                table: "TrackFiles");

            migrationBuilder.DropIndex(
                name: "IX_TrackFiles_Source",
                table: "TrackFiles");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "TrackFiles");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "TrackFiles");

            migrationBuilder.DropColumn(
                name: "ExternalMetadataJson",
                table: "TrackFiles");

            migrationBuilder.DropColumn(
                name: "IsPinnedByUser",
                table: "TrackFiles");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "TrackFiles");
        }
    }
}
