using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopicalBirdAPI.Models
{
    [Table("media")]
    public class Media
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [ForeignKey("Post")]
        [Column("posts_id")]
        public Guid? PostId { get; set; }

        public Posts? Post { get; set; }

        [Column("content_url")]
        public string? ContentUrl { get; set; }

        [Column("alt_text")]
        public string? AltText { get; set; }
    }
}
