using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dabirkhane.Migrations
{
    /// <inheritdoc />
    public partial class reply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reply_tbl_Recivers_tbl_ReciverId",
                table: "Reply_tbl");

            migrationBuilder.RenameColumn(
                name: "ReciverId",
                table: "Reply_tbl",
                newName: "SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_Reply_tbl_ReciverId",
                table: "Reply_tbl",
                newName: "IX_Reply_tbl_SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reply_tbl_Recivers_tbl_SenderId",
                table: "Reply_tbl",
                column: "SenderId",
                principalTable: "Recivers_tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reply_tbl_Recivers_tbl_SenderId",
                table: "Reply_tbl");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "Reply_tbl",
                newName: "ReciverId");

            migrationBuilder.RenameIndex(
                name: "IX_Reply_tbl_SenderId",
                table: "Reply_tbl",
                newName: "IX_Reply_tbl_ReciverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reply_tbl_Recivers_tbl_ReciverId",
                table: "Reply_tbl",
                column: "ReciverId",
                principalTable: "Recivers_tbl",
                principalColumn: "Id");
        }
    }
}
