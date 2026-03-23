using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaBrowser.Migrations
{
    /// <inheritdoc />
    public partial class Chapters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "parent_id",
                table: "media",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "start",
                table: "media",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "parent_id",
                table: "media");

            migrationBuilder.DropColumn(
                name: "start",
                table: "media");
        }
    }
}
