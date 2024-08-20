using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dabirkhane.Migrations
{
    /// <inheritdoc />
    public partial class second2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Messages_tbl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Messages_tbl",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
