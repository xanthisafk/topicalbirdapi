using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.DTO
{
    public class UpdateNestRequest
    {
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string? DisplayName { get; set; }

        public IFormFile? Icon { get; set; }
    }
}
