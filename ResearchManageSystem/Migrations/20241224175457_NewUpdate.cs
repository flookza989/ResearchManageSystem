using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchManageSystem.Migrations
{
    /// <inheritdoc />
    public partial class NewUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Research",
                newName: "Keywords");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ResearchStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Research",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Research",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Research",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AdvisorId",
                table: "Research",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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

            migrationBuilder.AddColumn<int>(
                name: "ResearcherId",
                table: "Research",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Research",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Research_ResearcherId",
                table: "Research",
                column: "ResearcherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Research_Users_ResearcherId",
                table: "Research",
                column: "ResearcherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Research_Users_ResearcherId",
                table: "Research");

            migrationBuilder.DropIndex(
                name: "IX_Research_ResearcherId",
                table: "Research");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ResearchStudents");

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
                name: "ResearcherId",
                table: "Research");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Research");

            migrationBuilder.RenameColumn(
                name: "Keywords",
                table: "Research",
                newName: "Description");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Research",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Research",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Research",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "AdvisorId",
                table: "Research",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResearchId = table.Column<int>(type: "int", nullable: false),
                    AdvisorFilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Chapter = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StudentFilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_Research_ResearchId",
                        column: x => x.ResearchId,
                        principalTable: "Research",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ResearchId",
                table: "Submissions",
                column: "ResearchId");
        }
    }
}
