using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.PostDTO
{
    public class CreatePostRequest
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required.", AllowEmptyStrings = true)]
        public string Content { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Nest name is required.")]
        public string NestTitle { get; set; }

        public List<IFormFile> Images { get; set; } = [];
        public List<string> Alts { get; set; } = [];

        public List<CreatePostMediaItem> MediaItems()
        {
            List<CreatePostMediaItem> items = [];
            for (int i=0; i<Images.Count; i++)
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
