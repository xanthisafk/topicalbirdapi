using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Data.DTO.MediaDTO
{
    public class MediaResponse
    {
        public string? Url { get; set; }
        public string? Alt { get; set; }

        public static MediaResponse FromMedia(Media m)
        {
            return new MediaResponse
            {
                Url = m.ContentUrl,
                Alt = m.AltText
            };
        }
    }
}
