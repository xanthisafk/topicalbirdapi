using System.ComponentModel.DataAnnotations;
using TopicalBirdAPI.Data.Constants;

namespace TopicalBirdAPI.Data.DTO.NestDTO
{
    /// <summary>
    /// Helper model for creating new Nest
    /// </summary>
    public class CreateNestRequest
    {

        /// <summary>
        /// Title of nest. Also used as slug
        /// </summary>
        [Required(ErrorMessage = ErrorMessages.FieldRequired + "Nest Title")]
        [MinLength(3, ErrorMessage = ErrorMessages.NestTitleTooSmall)]
        [MaxLength(50, ErrorMessage = ErrorMessages.NestTitleTooBig)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Description of Nest
        /// </summary>
        [MaxLength(500, ErrorMessage = ErrorMessages.NestDescriptionTooLong)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Display name of Nest
        /// </summary>
        [MaxLength(100, ErrorMessage = ErrorMessages.DisplayNameTooLarge)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Icon or Logo of Nest. Only Images and GIF allowed. Max 5MB.
        /// </summary>
        public IFormFile? Icon { get; set; }
    }
}
