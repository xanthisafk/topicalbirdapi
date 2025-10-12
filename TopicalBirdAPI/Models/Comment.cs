using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopicalBirdAPI.Models
{
    [Table("comment")]
    public class Comment
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [ForeignKey("Post")]
        [Column("posts_id")]
        public Guid? PostId { get; set; }
        public Posts? Post { get; set; }

        [ForeignKey("Author")]
        [Column("author_id")]
        public Guid? AuthorId { get; set; }
        public Users? Author { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
