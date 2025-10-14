using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.AuthDTO
{
    public class UserLoginRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }
}
