using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using TopicalBirdAPI.Data.Constants;

namespace TopicalBirdAPI.Data.DTO.UsersDTO
{
    public class CreateUserRequest
    {
        /// <summary>
        /// User email address
        /// </summary>
        [Required(ErrorMessage = ErrorMessages.FieldRequired + "Email")]
        [EmailAddress(ErrorMessage = ErrorMessages.UserMalformedEmail)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Must be at least 8 char long and must consist of uppercase, lowercase, special and numerical char.
        /// </summary>
        [Required(ErrorMessage = ErrorMessages.FieldRequired + "Password")]
        [DataType(DataType.Password)]
        [RegularExpression(DTOConstants.PasswordRegex, ErrorMessage = ErrorMessages.UserMalformedPassword)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Username. Case is ignored.
        /// </summary>
        [Required(ErrorMessage = ErrorMessages.FieldRequired + "Username")]
        [MaxLength(50, ErrorMessage = ErrorMessages.HandleTooLarge)]
        public string Handle { get; set; } = string.Empty;

        /// <summary>
        /// Display name. Case is kept.
        /// </summary>
        [MaxLength(100, ErrorMessage = ErrorMessages.DisplayNameTooLarge)]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Avatar image. Allowed types: .JPG, .PNG, .WEBP, .GIF. Maximum 5MB.
        /// </summary>
        public IFormFile? Icon { get; set; }
    }
}
