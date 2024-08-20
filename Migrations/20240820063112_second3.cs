using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dabirkhane.Migrations
{
    /// <inheritdoc />
    public partial class second3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status4Sender",
                table: "Messages_tbl",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status4Sender",
                table: "Messages_tbl");
        }
    }
}
