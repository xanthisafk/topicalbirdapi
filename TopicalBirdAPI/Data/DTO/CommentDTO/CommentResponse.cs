using TopicalBirdAPI.Data.DTO.UsersDTO;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Data.DTO.CommentDTO
{
    public class CommentResponse
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserResponse Author { get; set; }

        public static CommentResponse FromComment(Comment cmt)
        {
            return new CommentResponse
            {
                Id = cmt.Id,
                Content = cmt.Content,
                CreatedAt = cmt.CreatedAt,
                Author = UserResponse.FromUser(cmt.Author)
            };
        }
    }

}
