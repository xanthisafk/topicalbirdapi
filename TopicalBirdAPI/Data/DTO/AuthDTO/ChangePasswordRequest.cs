using System.ComponentModel.DataAnnotations;
using TopicalBirdAPI.Data.Constants;

namespace TopicalBirdAPI.Data.DTO.AuthDTO
{
    /// <summary>
    /// Data Transfer Object (DTO) used for requesting a password change.
    /// </summary>
    public class ChangePasswordRequest
    {

        /// <summary>
        /// The user's current password. This is required for security verification.
        /// </summary>
        /// <example>P@sswOrd123</example>
        [Required(ErrorMessage = ErrorMessages.FieldRequired + "OLD PASSWORD")]
        [DataType(DataType.Password, ErrorMessage = ErrorMessages.UserMalformedPassword)]
        [RegularExpression(DTOConstants.PasswordRegex, ErrorMessage = ErrorMessages.UserMalformedPassword)]
        public string OldPassword { get; set; } = string.Empty;

        /// <summary>
        /// The desired new password for the user's account.
        /// </summary>
        /// <example>NewP@ssw0rd456</example>
        [Required(ErrorMessage = ErrorMessages.FieldRequired + "OLD PASSWORD")]
        [DataType(DataType.Password, ErrorMessage = ErrorMessages.UserMalformedPassword)]
        [RegularExpression(DTOConstants.PasswordRegex, ErrorMessage = ErrorMessages.UserMalformedPassword)]
        public string NewPassword { get; set; } = string.Empty;
    }
}