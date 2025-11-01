using System.ComponentModel.DataAnnotations;
using TopicalBirdAPI.Data.Constants;

namespace TopicalBirdAPI.Data.DTO.PostDTO
{
    public class PostUpdateRequest
    {
        [MaxLength(10000, ErrorMessage = ErrorMessages.PostContentTooBig)]
        public string? Content { get; set; }
    }
}
