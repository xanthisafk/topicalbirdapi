using TopicalBirdAPI.Data.DTO.UsersDTO;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Data.DTO.NestDTO
{
    public class NestResponse
    {
        public Guid Id { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public UserResponse? Moderator { get; set; }

        public static NestResponse FromNest(Nest nest, bool admin = false)
        {
            ArgumentNullException.ThrowIfNull(nest, nameof(nest));

            return new NestResponse
            {
                Id = nest.Id,
                Icon = nest.Icon ?? string.Empty,
                Description = nest.Description ?? string.Empty,
                DisplayName = nest.DisplayName ?? string.Empty,
                CreatedAt = nest.CreatedAt,
                Title = nest.Title ?? string.Empty,
                Moderator = nest.Moderator != null
                    ? UserResponse.FromUser(nest.Moderator, admin)
                    : null
            };
        }

    }
}
