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
        public bool HasVoted { get; set; } = false;
        public bool ByModerator { get; set; } = false;
        public bool IsModerator { get; set; } = false;
        public int Votes { get; set; }
        public int Comments { get; set; }

        public static PostResponse FromPost(Posts p, Users? currentUser = null)
        {
            int upvotes, downvotes;
            upvotes = p.Votes?.Where(v => v.VoteValue == 1).Count() ?? 0;
            downvotes = p.Votes?.Where(v => v.VoteValue == -1).Count() ?? 0;

            bool hasVoted = currentUser != null && p.Votes?.FirstOrDefault(v => v.UserId == currentUser.Id) != null;
            bool isModerator = currentUser != null && currentUser.Id == p.Nest?.ModeratorId;
            bool byModerator = p.AuthorId == p.Nest?.ModeratorId;


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
                Comments = p.Comments.Where(c => !c.IsDeleted).ToList().Count,
                ByModerator = byModerator,
                IsModerator = isModerator,
                HasVoted = hasVoted,
            };
        }
    }
}
