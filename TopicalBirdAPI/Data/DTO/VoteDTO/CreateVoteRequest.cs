using System.ComponentModel.DataAnnotations;
using TopicalBirdAPI.Data.Constants;

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
        [Required(ErrorMessage = ErrorMessages.FieldRequired + "Value")]
        [Range(-1, 1, ErrorMessage = "Vote value must be -1 for downvote, 0 to remove vote, or 1 for upvote.")]
        public int VoteValue { get; set; }
    }
}
