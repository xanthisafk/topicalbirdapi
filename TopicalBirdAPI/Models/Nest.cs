using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopicalBirdAPI.Models
{
    [Table("nest")]
    public class Nest
    {
        [Key, Column("id")]
        public Guid Id { get; set; }

        [Required, Column("title"), StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Column("display_name")]
        public string? DisplayName { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; } = string.Empty;

        [Column("icon")]
        public string Icon { get; set; } = "/content/assets/defaults/nest_256.png";


        [ForeignKey("Moderator")]
        [Column("moderator_id")]
        public Guid ModeratorId { get; set; }

        public Users? Moderator { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Posts>? Posts { get; set; }
    }
}