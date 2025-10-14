using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopicalBirdAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddHandleToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "handle",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "handle",
                table: "users");
        }
    }
}
