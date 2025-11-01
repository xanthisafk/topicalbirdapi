using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopicalBirdAPI.Migrations
{
    /// <inheritdoc />
    public partial class CascadeUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_comment_users_author_id",
                table: "comment");

            migrationBuilder.DropForeignKey(
                name: "FK_media_posts_posts_id",
                table: "media");

            migrationBuilder.DropForeignKey(
                name: "FK_nest_users_moderator_id",
                table: "nest");

            migrationBuilder.AddForeignKey(
                name: "FK_comment_users_author_id",
                table: "comment",
                column: "author_id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_media_posts_posts_id",
                table: "media",
                column: "posts_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_nest_users_moderator_id",
                table: "nest",
                column: "moderator_id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_comment_users_author_id",
                table: "comment");

            migrationBuilder.DropForeignKey(
                name: "FK_media_posts_posts_id",
                table: "media");

            migrationBuilder.DropForeignKey(
                name: "FK_nest_users_moderator_id",
                table: "nest");

            migrationBuilder.AddForeignKey(
                name: "FK_comment_users_author_id",
                table: "comment",
                column: "author_id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_media_posts_posts_id",
                table: "media",
                column: "posts_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_nest_users_moderator_id",
                table: "nest",
                column: "moderator_id",
                principalTable: "users",
                principalColumn: "Id");
        }
    }
}
