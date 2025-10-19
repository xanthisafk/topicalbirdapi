using TopicalBirdAPI.Data.DTO.PostDTO;
using TopicalBirdAPI.Data.DTO.UsersDTO;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Data.DTO.VoteDTO
{
    public class VoteResponse
    {
        public int VoteValue { get; set; }
        public PostResponse? Post { get; set; }
        public UserResponse? User { get; set; }

        public static VoteResponse FromVote(PostVote v)
        {
            return new VoteResponse
            {
                VoteValue = v.VoteValue,
                Post = v.Post != null ? PostResponse.FromPost(v.Post) : null,
                User = v.User != null ? UserResponse.FromUser(v.User) : null
            };
        }
    }
}
