using System.ComponentModel.DataAnnotations;
using TopicalBirdAPI.Data.Constants;

namespace TopicalBirdAPI.Data.DTO.PostDTO
{
    public class CreatePostRequest
    {
        [Required(ErrorMessage = ErrorMessages.FieldRequired + "Post Title")]
        [MinLength(3, ErrorMessage =ErrorMessages.PostTitleTooSmall)]
        [MaxLength(500, ErrorMessage = ErrorMessages.PostTitleTooBig)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = ErrorMessages.FieldRequired + "Content", AllowEmptyStrings = true)]
        [MaxLength(10000, ErrorMessage = ErrorMessages.PostContentTooBig)]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = ErrorMessages.FieldRequired + "Nest Title")]
        public string NestTitle { get; set; } = string.Empty;

        public List<IFormFile> Images { get; set; } = [];
        public List<string> Alts { get; set; } = [];

        public List<CreatePostMediaItem> MediaItems()
        {
            List<CreatePostMediaItem> items = [];
            for (int i = 0; i < Images.Count; i++)
            {
                items.Add(new CreatePostMediaItem
                {
                    Image = Images[i],
                    AltText = ((Alts.Count > i) && (Alts[i] != null)) ? Alts[i].Trim() : string.Empty,
                });
            }

            return items;
        }
    }
}
