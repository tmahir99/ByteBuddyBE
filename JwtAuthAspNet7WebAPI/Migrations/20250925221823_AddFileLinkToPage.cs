using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtAuthAspNet7WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddFileLinkToPage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileId",
                table: "Pages",
                type: "bigint",
                nullable: true); // Make nullable, remove default

            migrationBuilder.CreateIndex(
                name: "IX_Pages_FileId",
                table: "Pages",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_Files_FileId",
                table: "Pages",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "FileId",
                onDelete: ReferentialAction.SetNull); // SetNull for nullable FK
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pages_Files_FileId",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_FileId",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Pages");
        }
    }
}
