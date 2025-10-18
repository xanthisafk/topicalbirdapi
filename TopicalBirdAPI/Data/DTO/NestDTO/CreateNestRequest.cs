using System.ComponentModel.DataAnnotations;

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
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Description of Nest
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Display name of Nest
        /// </summary>
        [MaxLength(50)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Icon or Logo of Nest. Only Images and GIF allowed. Max 5MB.
        /// </summary>
        public IFormFile? Icon { get; set; }
    }
}
