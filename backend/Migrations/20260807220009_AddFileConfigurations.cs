using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nitevault.Migrations
{
    /// <inheritdoc />
    public partial class AddFileConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FilePart_FileEntity_FileId",
                table: "FilePart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FilePart",
                table: "FilePart");

            migrationBuilder.DropIndex(
                name: "IX_FilePart_FileId",
                table: "FilePart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileEntity",
                table: "FileEntity");

            migrationBuilder.RenameTable(
                name: "FilePart",
                newName: "FileParts");

            migrationBuilder.RenameTable(
                name: "FileEntity",
                newName: "Files");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "FileParts",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Files",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Files",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileParts",
                table: "FileParts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Files",
                table: "Files",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FileParts_FileId_PartNumber",
                table: "FileParts",
                columns: new[] { "FileId", "PartNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Files_FolderId",
                table: "Files",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_UserId",
                table: "Files",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileParts_Files_FileId",
                table: "FileParts",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileParts_Files_FileId",
                table: "FileParts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Files",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_FolderId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_UserId",
                table: "Files");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileParts",
                table: "FileParts");

            migrationBuilder.DropIndex(
                name: "IX_FileParts_FileId_PartNumber",
                table: "FileParts");

            migrationBuilder.RenameTable(
                name: "Files",
                newName: "FileEntity");

            migrationBuilder.RenameTable(
                name: "FileParts",
                newName: "FilePart");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "FileEntity",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "FileEntity",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "FilePart",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileEntity",
                table: "FileEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FilePart",
                table: "FilePart",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FilePart_FileId",
                table: "FilePart",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_FilePart_FileEntity_FileId",
                table: "FilePart",
                column: "FileId",
                principalTable: "FileEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
