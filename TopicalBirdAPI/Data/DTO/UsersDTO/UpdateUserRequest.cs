using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.UsersDTO
{
    public class UpdateUserRequest
    {
        public string? DisplayName { get; set; }
        public IFormFile? Icon { get; set; }
    }
}
