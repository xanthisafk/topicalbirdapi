using System.ComponentModel.DataAnnotations;

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
        [Required]
        [EmailAddress] // Added for better validation/clarity in Swagger
        public string Email { get; set; }

        /// <summary>
        /// The user's password.
        /// </summary>
        /// <example>P@sswOrd123</example>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// A flag indicating whether the system should remember the user's login session. Defaults to false.
        /// </summary>
        /// <example>true</example>
        public bool RememberMe { get; set; } = false;
    }
}