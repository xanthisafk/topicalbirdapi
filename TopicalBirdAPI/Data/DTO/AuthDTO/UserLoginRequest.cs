using System.ComponentModel.DataAnnotations;
using TopicalBirdAPI.Data.Constants;

namespace TopicalBirdAPI.Data.DTO.AuthDTO
{
    /// <summary>
    /// Helper class used to submit user credentials for login authentication.
    /// </summary>
    public class UserLoginRequest
    {
        /// <summary>
        /// The email address of the user. This is used as the primary identifier for login.
        /// </summary>
        /// <example>user.name@example.com</example>
        [Required(ErrorMessage = ErrorMessages.FieldRequired + "Email")]
        [EmailAddress(ErrorMessage = ErrorMessages.UserMalformedEmail)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The user's password.
        /// </summary>
        /// <example>P@sswOrd123</example>
        [Required(ErrorMessage = ErrorMessages.FieldRequired + "Password")]
        [DataType(DataType.Password, ErrorMessage = ErrorMessages.UserMalformedPassword)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// A flag indicating whether the system should remember the user's login session. Defaults to false.
        /// </summary>
        /// <example>true</example>
        public bool RememberMe { get; set; } = false;
    }
}