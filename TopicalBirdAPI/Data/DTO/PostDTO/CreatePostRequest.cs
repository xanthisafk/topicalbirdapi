using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.PostDTO
{
    public class CreatePostRequest
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty;

        public Guid? NestId { get; set; }

        public List<CreatePostMediaItem> MediaItems { get; set; } = [];
    }
}
