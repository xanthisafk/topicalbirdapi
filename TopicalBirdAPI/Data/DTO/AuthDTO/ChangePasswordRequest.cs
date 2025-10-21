using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.AuthDTO
{
    /// <summary>
    /// Data Transfer Object (DTO) used for requesting a password change.
    /// </summary>
    public class ChangePasswordRequest
    {

        private const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$";
        private const string PasswordError =
            "Password must be at least 6 characters long and contain at least one uppercase letter, " +
            "one lowercase letter, one digit, and one non-alphanumeric character (symbol).";


        /// <summary>
        /// The user's current password. This is required for security verification.
        /// </summary>
        /// <example>P@sswOrd123</example>
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(PasswordRegex, ErrorMessage = PasswordError)]
        public string OldPassword { get; set; }

        /// <summary>
        /// The desired new password for the user's account.
        /// </summary>
        /// <example>NewP@ssw0rd456</example>
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(PasswordRegex, ErrorMessage = PasswordError)]
        public string NewPassword { get; set; }

        /// <summary>
        /// A confirmation of the new password, which must match the value in the <see cref="NewPassword"/> field.
        /// </summary>
        /// <example>NewP@ssw0rd456</example>
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(PasswordRegex, ErrorMessage = PasswordError)]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}