using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.CommentDTO
{
    public class CreateCommentRequest
    {
        public Guid PostId { get; set; }
        [Required, MinLength(1), MaxLength(10000)]
        public string Content { get; set; }
    }
}
