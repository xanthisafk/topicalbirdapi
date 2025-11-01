namespace TopicalBirdAPI.Data.DTO.PostDTO
{
    public class CreatePostMediaItem
    {
        public IFormFile? Image { get; set; }
        public string AltText { get; set; } = string.Empty;
    }
}
