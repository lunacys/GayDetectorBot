using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GayDetectorBot.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToSavedFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TgUser_Chat_LastActivityChatId",
                table: "TgUser");

            migrationBuilder.DropIndex(
                name: "IX_TgUser_LastActivityChatId",
                table: "TgUser");

            migrationBuilder.DropColumn(
                name: "LastActivityChatId",
                table: "TgUser");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "SavedFile",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SavedFile");

            migrationBuilder.AddColumn<long>(
                name: "LastActivityChatId",
                table: "TgUser",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TgUser_LastActivityChatId",
                table: "TgUser",
                column: "LastActivityChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_TgUser_Chat_LastActivityChatId",
                table: "TgUser",
                column: "LastActivityChatId",
                principalTable: "Chat",
                principalColumn: "ChatId");
        }
    }
}
