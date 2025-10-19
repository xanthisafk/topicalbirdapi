using System.ComponentModel.DataAnnotations;

namespace TopicalBirdAPI.Data.DTO.VoteDTO
{
    /// <summary>
    /// Helper function for creating vote
    /// </summary>
    public class CreateVoteRequest
    {
        /// <summary>
        /// Score given. -1 for downvote, +1 for upvote, 0 to remove vote
        /// </summary>
        [Required, MinLength(-1), MaxLength(1)]
        public int VoteValue {  get; set; }
    }
}
