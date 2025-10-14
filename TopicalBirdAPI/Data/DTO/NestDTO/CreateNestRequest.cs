using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.NestDTO
{
    public class CreateNestRequest
    {
        [Required]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string DisplayName { get; set; } = string.Empty;
        public IFormFile? Icon { get; set; }
    }
}
