using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.DTO
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsBanned { get; set; } = false;

        public static UserResponse FromUser(Users user, bool admin)
        {
            bool shouldExposeSensitiveData = !user.IsBanned || admin;

            return new UserResponse
            {
                Id = user.Id,
                IsBanned = user.IsBanned,
                DisplayName = shouldExposeSensitiveData ? user.DisplayName : null,
                Icon = shouldExposeSensitiveData ? user.Icon : null,
                CreatedAt = shouldExposeSensitiveData ? user.CreatedAt : null,
                IsAdmin = shouldExposeSensitiveData ? user.IsAdmin : false,
                Email = admin ? user.Email : null,
            };
        }

    }
}
