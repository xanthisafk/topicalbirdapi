using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopicalBirdAPI.Models
{
    [Table("post_votes")]
    public class PostVote
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("is_upvote")]
        public bool IsUpvote { get; set; } = false;

        [ForeignKey("Post")]
        [Column("post_id")]
        public Guid PostId { get; set; }
        public Posts Post { get; set; }

        [ForeignKey("User")]
        [Column("user_id")]
        public Guid UserId { get; set; }
        public Users User { get; set; }

        // +1 for upvote, -1 for downvote
        [Column("vote_value")]
        public int VoteValue { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
