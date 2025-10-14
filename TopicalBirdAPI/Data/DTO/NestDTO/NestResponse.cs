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
        public ICollection<Posts> Posts { get; set; } // Todo: replace with PostResponse when working on PostsController

        public static NestResponse FromNest(Nest nest, bool includePosts = false, bool admin = false)
        {

            var res = new NestResponse
            {
                Id = nest.Id,
                Icon = nest.Icon,
                Description = nest.Description,
                DisplayName = nest.DisplayName,
                CreatedAt = nest.CreatedAt,
                Title = nest.Title,
                Moderator = UserResponse.FromUser(nest.Moderator, admin),
                Posts = includePosts ? nest.Posts : null
            };

            return res;
        }
    }
}
