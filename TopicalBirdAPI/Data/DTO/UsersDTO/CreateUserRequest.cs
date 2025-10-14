using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.UsersDTO
{
    public class CreateUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [MaxLength(50)]
        public string Handle { get; set; }

        [MaxLength(100)]
        public string? DisplayName { get; set; }

        public IFormFile? Icon { get; set; }
    }
}
