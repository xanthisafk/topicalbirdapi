using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.UsersDTO
{
    public class CreateUserRequest
    {
        /// <summary>
        /// User email address
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Must be at least 8 char long and must consist of uppercase, lowercase, special and numerical char.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Username. Case is ignored.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Handle { get; set; }

        /// <summary>
        /// Display name. Case is kept.
        /// </summary>
        [MaxLength(100)]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Avatar image. Allowed types: .JPG, .PNG, .WEBP, .GIF. Maximum 5MB.
        /// </summary>
        public IFormFile? Icon { get; set; }
    }
}
