using System.ComponentModel.DataAnnotations;
using TopicalBirdAPI.Data.Constants;

namespace TopicalBirdAPI.Data.DTO.NestDTO
{
    public class UpdateNestRequest
    {
        [MaxLength(500, ErrorMessage = ErrorMessages.NestDescriptionTooLong)]
        public string? Description { get; set; }

        [MaxLength(100, ErrorMessage = ErrorMessages.DisplayNameTooLarge)]
        public string? DisplayName { get; set; }

        public IFormFile? Icon { get; set; }
    }
}
