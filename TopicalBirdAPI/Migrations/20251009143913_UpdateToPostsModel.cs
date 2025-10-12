using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopicalBirdAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToPostsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_comment_posts_PostsId",
                table: "comment");

            migrationBuilder.DropForeignKey(
                name: "FK_comment_posts_posts_id1",
                table: "comment");

            migrationBuilder.DropForeignKey(
                name: "FK_comment_users_author_id1",
                table: "comment");

            migrationBuilder.DropForeignKey(
                name: "FK_media_posts_PostsId",
                table: "media");

            migrationBuilder.DropForeignKey(
                name: "FK_media_posts_posts_id1",
                table: "media");

            migrationBuilder.DropForeignKey(
                name: "FK_posts_users_author_id1",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "IX_media_posts_id1",
                table: "media");

            migrationBuilder.DropIndex(
                name: "IX_media_PostsId",
                table: "media");

            migrationBuilder.DropIndex(
                name: "IX_comment_author_id1",
                table: "comment");

            migrationBuilder.DropIndex(
                name: "IX_comment_posts_id1",
                table: "comment");

            migrationBuilder.DropIndex(
                name: "IX_comment_PostsId",
                table: "comment");

            migrationBuilder.DropColumn(
                name: "PostsId",
                table: "media");

            migrationBuilder.DropColumn(
                name: "posts_id1",
                table: "media");

            migrationBuilder.DropColumn(
                name: "PostsId",
                table: "comment");

            migrationBuilder.DropColumn(
                name: "author_id1",
                table: "comment");

            migrationBuilder.DropColumn(
                name: "posts_id1",
                table: "comment");

            migrationBuilder.RenameColumn(
                name: "author_id1",
                table: "posts",
                newName: "nest_id");

            migrationBuilder.RenameIndex(
                name: "IX_posts_author_id1",
                table: "posts",
                newName: "IX_posts_nest_id");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "posts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "posts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "post_votes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vote_value = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_votes", x => x.id);
                    table.ForeignKey(
                        name: "FK_post_votes_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_post_votes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_posts_author_id",
                table: "posts",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_media_posts_id",
                table: "media",
                column: "posts_id");

            migrationBuilder.CreateIndex(
                name: "IX_comment_author_id",
                table: "comment",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_comment_posts_id",
                table: "comment",
                column: "posts_id");

            migrationBuilder.CreateIndex(
                name: "IX_post_votes_post_id_user_id",
                table: "post_votes",
                columns: new[] { "post_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_post_votes_user_id",
                table: "post_votes",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_comment_posts_posts_id",
                table: "comment",
                column: "posts_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_posts_nest_nest_id",
                table: "posts",
                column: "nest_id",
                principalTable: "nest",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_posts_users_author_id",
                table: "posts",
                column: "author_id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_comment_posts_posts_id",
                table: "comment");

            migrationBuilder.DropForeignKey(
                name: "FK_comment_users_author_id",
                table: "comment");

            migrationBuilder.DropForeignKey(
                name: "FK_media_posts_posts_id",
                table: "media");

            migrationBuilder.DropForeignKey(
                name: "FK_posts_nest_nest_id",
                table: "posts");

            migrationBuilder.DropForeignKey(
                name: "FK_posts_users_author_id",
                table: "posts");

            migrationBuilder.DropTable(
                name: "post_votes");

            migrationBuilder.DropIndex(
                name: "IX_posts_author_id",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "IX_media_posts_id",
                table: "media");

            migrationBuilder.DropIndex(
                name: "IX_comment_author_id",
                table: "comment");

            migrationBuilder.DropIndex(
                name: "IX_comment_posts_id",
                table: "comment");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "posts");

            migrationBuilder.RenameColumn(
                name: "nest_id",
                table: "posts",
                newName: "author_id1");

            migrationBuilder.RenameIndex(
                name: "IX_posts_nest_id",
                table: "posts",
                newName: "IX_posts_author_id1");

            migrationBuilder.AddColumn<Guid>(
                name: "PostsId",
                table: "media",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "posts_id1",
                table: "media",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostsId",
                table: "comment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "author_id1",
                table: "comment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "posts_id1",
                table: "comment",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_posts_id1",
                table: "media",
                column: "posts_id1");

            migrationBuilder.CreateIndex(
                name: "IX_media_PostsId",
                table: "media",
                column: "PostsId");

            migrationBuilder.CreateIndex(
                name: "IX_comment_author_id1",
                table: "comment",
                column: "author_id1");

            migrationBuilder.CreateIndex(
                name: "IX_comment_posts_id1",
                table: "comment",
                column: "posts_id1");

            migrationBuilder.CreateIndex(
                name: "IX_comment_PostsId",
                table: "comment",
                column: "PostsId");

            migrationBuilder.AddForeignKey(
                name: "FK_comment_posts_PostsId",
                table: "comment",
                column: "PostsId",
                principalTable: "posts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_comment_posts_posts_id1",
                table: "comment",
                column: "posts_id1",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_comment_users_author_id1",
                table: "comment",
                column: "author_id1",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_media_posts_PostsId",
                table: "media",
                column: "PostsId",
                principalTable: "posts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_media_posts_posts_id1",
                table: "media",
                column: "posts_id1",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_posts_users_author_id1",
                table: "posts",
                column: "author_id1",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
