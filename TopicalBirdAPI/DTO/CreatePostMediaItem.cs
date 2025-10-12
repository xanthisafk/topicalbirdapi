namespace TopicalBirdAPI.DTO
{
    public class CreatePostMediaItem
    {
        public IFormFile Image { get; set; }
        public string AltText { get; set; } = String.Empty;
    }
}
