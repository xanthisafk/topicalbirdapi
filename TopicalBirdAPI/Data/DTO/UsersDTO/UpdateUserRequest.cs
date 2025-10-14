using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.UsersDTO
{
    public class UpdateUserRequest
    {
        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Handle { get; set; } = string.Empty;
        public IFormFile? Icon { get; set; }
    }
}
