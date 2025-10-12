using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.DTO
{
    public class CommentResponse
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public AuthorInfo Author { get; set; }

        public class AuthorInfo
        {
            public Guid Id { get; set; }
            public string? Icon { get; set; }
            public string? UserName { get; set; }
            public bool IsAdmin { get; set; }
        }

        public static CommentResponse FromComment(Comment cmt)
        {
            return new CommentResponse
            {
                Id = cmt.Id,
                Content = cmt.Content,
                CreatedAt = cmt.CreatedAt,
                Author = new AuthorInfo
                {
                    Id = cmt.Author!.Id,
                    Icon = cmt.Author.Icon,
                    UserName = cmt.Author.UserName,
                    IsAdmin = cmt.Author.IsAdmin
                }
            };
        }
    }

}
