using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.UsersDTO
{
    /// <summary>
    /// Represents the data required to update a user's profile.
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// The new public display name for the user.
        /// </summary>
        /// <example>Shruti Thakur</example>
        public string? DisplayName { get; set; }

        /// <summary>
        /// The new profile image file for the user. Maximum 5MB. Only images and GIFs allowed.
        /// </summary>
        /// <example>photo.png</example>
        public IFormFile? Icon { get; set; }
    }
}