using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopicalBirdAPI.Models
{
    [Table("posts")]
    public class Posts
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [ForeignKey("Author")]
        [Column("author_id")]
        public Guid? AuthorId { get; set; }
        public Users? Author { get; set; }

        [ForeignKey("Nest")]
        [Column("nest_id")]
        public Guid? NestId { get; set; }
        public Nest? Nest { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Media>? MediaItems { get; set; }
        public ICollection<PostVote>? Votes { get; set; }
    }
}
