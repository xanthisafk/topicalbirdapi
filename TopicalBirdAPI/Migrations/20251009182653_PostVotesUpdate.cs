using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopicalBirdAPI.Migrations
{
    /// <inheritdoc />
    public partial class PostVotesUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_upvote",
                table: "post_votes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_upvote",
                table: "post_votes");
        }
    }
}
