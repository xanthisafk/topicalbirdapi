using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.AuthDTO
{
    public class ChangePasswordRequest
    {
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}
