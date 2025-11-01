using TopicalBirdAPI.Data.DTO.MediaDTO;
using TopicalBirdAPI.Data.DTO.NestDTO;
using TopicalBirdAPI.Data.DTO.UsersDTO;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Data.DTO.PostDTO
{
    public class PostResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public UserResponse? Author { get; set; }
        public NestResponse? Nest { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.MinValue;
        public DateTime? UpdatedAt { get; set; }
        public List<MediaResponse>? Photos { get; set; }
        public int Votes { get; set; }
        public int Comments { get; set; }

        public static PostResponse FromPost(Posts p)
        {
            int upvotes, downvotes;
            upvotes = p.Votes?.Where(v => v.VoteValue == 1).Count() ?? 0;
            downvotes = p.Votes?.Where(v => v.VoteValue == -1).Count() ?? 0;
            var photos = p.MediaItems?.Select(MediaResponse.FromMedia).ToList();

            ArgumentNullException.ThrowIfNull(p.Author, nameof(p.Author));
            ArgumentNullException.ThrowIfNull(p.Nest, nameof(p.Nest));
            ArgumentNullException.ThrowIfNull(p.Comments, nameof(p.Comments));


            return new PostResponse
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                Author = UserResponse.FromUser(p.Author),
                Nest = NestResponse.FromNest(p.Nest),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Photos = photos,
                Votes = upvotes - downvotes,
                Comments = p.Comments.Count
            };
        }
    }
}
