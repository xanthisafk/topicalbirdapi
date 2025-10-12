using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopicalBirdAPI.Models
{
    [Table("users")]
    public class Users : IdentityUser<Guid>
    {
        [Column("display_name")]
        public string? DisplayName { get; set; }

        [Column("icon")]
        public string? Icon { get; set; } = "/content/assets/defaults/pp_256.png";

        [Column("is_admin")]
        public bool IsAdmin { get; set; } = false;

        [Column("is_banned")]
        public bool IsBanned { get; set; } = false;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}
