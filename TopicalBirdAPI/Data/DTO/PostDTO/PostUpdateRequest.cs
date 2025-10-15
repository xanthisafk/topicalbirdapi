using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.PostDTO
{
    public class PostUpdateRequest
    {
        [MaxLength(10000)]
        public string? Content { get; set; }
    }
}
