using TopicalBirdAPI.Data.DTO.PostDTO;
using TopicalBirdAPI.Data.DTO.UsersDTO;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Data.DTO.NestDTO
{
    public class NestResponse
    {
        public Guid Id { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; }
        public UserResponse Moderator { get; set; }

        public static NestResponse FromNest(Nest nest, bool admin = false)
        {
            bool getNest = nest.Moderator != null;
            var res = new NestResponse
            {
                Id = nest.Id,
                Icon = nest.Icon,
                Description = nest.Description,
                DisplayName = nest.DisplayName,
                CreatedAt = nest.CreatedAt,
                Title = nest.Title,
                Moderator = getNest ? UserResponse.FromUser(nest.Moderator, admin) : null,
            };

            return res;
        }
    }
}
