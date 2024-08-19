using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dabirkhane.Migrations
{
    /// <inheritdoc />
    public partial class first : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sms_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SmsCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TryCount = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sms_tbl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "smsToken_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_smsToken_tbl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Addres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NatinalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerconalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Profile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users_tbl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SerialNumberCode = table.Column<int>(type: "int", nullable: true),
                    SenderUserId = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BodyText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages_tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_tbl_Users_tbl_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users_tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Recivers_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReciverId = table.Column<int>(type: "int", nullable: true),
                    MessageId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isRead = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recivers_tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recivers_tbl_Messages_tbl_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages_tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Recivers_tbl_Users_tbl_ReciverId",
                        column: x => x.ReciverId,
                        principalTable: "Users_tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reply_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: true),
                    ReciverId = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BodyText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isRead = table.Column<bool>(type: "bit", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reply_tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reply_tbl_Messages_tbl_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages_tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reply_tbl_Recivers_tbl_ReciverId",
                        column: x => x.ReciverId,
                        principalTable: "Recivers_tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Files_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplyId = table.Column<int>(type: "int", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files_tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_tbl_Messages_tbl_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages_tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Files_tbl_Reply_tbl_ReplyId",
                        column: x => x.ReplyId,
                        principalTable: "Reply_tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Files_tbl_Users_tbl_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "Users_tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_tbl_CreatorUserId",
                table: "Files_tbl",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_tbl_MessageId",
                table: "Files_tbl",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_tbl_ReplyId",
                table: "Files_tbl",
                column: "ReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_tbl_SenderUserId",
                table: "Messages_tbl",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Recivers_tbl_MessageId",
                table: "Recivers_tbl",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Recivers_tbl_ReciverId",
                table: "Recivers_tbl",
                column: "ReciverId");

            migrationBuilder.CreateIndex(
                name: "IX_Reply_tbl_MessageId",
                table: "Reply_tbl",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Reply_tbl_ReciverId",
                table: "Reply_tbl",
                column: "ReciverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files_tbl");

            migrationBuilder.DropTable(
                name: "sms_tbl");

            migrationBuilder.DropTable(
                name: "smsToken_tbl");

            migrationBuilder.DropTable(
                name: "Reply_tbl");

            migrationBuilder.DropTable(
                name: "Recivers_tbl");

            migrationBuilder.DropTable(
                name: "Messages_tbl");

            migrationBuilder.DropTable(
                name: "Users_tbl");
        }
    }
}
