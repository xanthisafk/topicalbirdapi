using System.ComponentModel.DataAnnotations;

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
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        /// <summary>
        /// The desired new password for the user's account.
        /// </summary>
        /// <example>NewP@ssw0rd456</example>
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        /// <summary>
        /// A confirmation of the new password, which must match the value in the <see cref="NewPassword"/> field.
        /// </summary>
        /// <example>NewP@ssw0rd456</example>
        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}