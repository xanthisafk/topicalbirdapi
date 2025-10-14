using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.DTO
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

        public static NestResponse FromNest(Nest nest)
        {
            return new NestResponse
            {
                Id = nest.Id,
                Icon = nest.Icon,
                Description = nest.Description,
                DisplayName = nest.DisplayName,
                CreatedAt = nest.CreatedAt,
                Title = nest.Title,
                Moderator = new UserResponse
                {
                    Id = nest.Moderator.Id,
                    DisplayName = nest.Moderator.DisplayName,
                    Icon = nest.Moderator.Icon
                },
                Posts = nest.Posts
            };
        }
    }
}
