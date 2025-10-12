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
        public string Title { get; set; }

        [Column("display_name")]
        public string? DisplayName { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("icon")]
        public string? Icon { get; set; } = "/content/assets/defaults/nest_256.png";


        [ForeignKey("Moderator")]
        [Column("moderator_id")]
        public Guid? ModeratorId { get; set; }

        public Users? Moderator { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Posts>? Posts { get; set; }
    }
}