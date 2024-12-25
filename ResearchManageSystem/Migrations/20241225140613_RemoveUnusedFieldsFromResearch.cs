using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchManageSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedFieldsFromResearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Abstract",
                table: "Research");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "Research");

            migrationBuilder.DropColumn(
                name: "DocumentPath",
                table: "Research");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Research");

            migrationBuilder.DropColumn(
                name: "FundingSource",
                table: "Research");

            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "Research");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Research");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Abstract",
                table: "Research",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "Research",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DocumentPath",
                table: "Research",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Research",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FundingSource",
                table: "Research",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                table: "Research",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Research",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
