using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Data
{
    public class AppDbContext : IdentityDbContext<Users, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Nest> Nests { get; set; }
        public DbSet<Posts> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<PostVote> PostVotes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Users>().ToTable("users");
            builder.Entity<IdentityRole<Guid>>().ToTable("roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");

            // Posts -> Author
            builder.Entity<Posts>()
                .HasOne(p => p.Author)
                .WithMany()
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Posts -> Nest
            builder.Entity<Posts>()
                .HasOne(p => p.Nest)
                .WithMany(n => n.Posts)
                .HasForeignKey(p => p.NestId)
                .OnDelete(DeleteBehavior.SetNull);

            // Comment -> Post
            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment -> Author
            builder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Media -> Post
            builder.Entity<Media>()
                .HasOne(m => m.Post)
                .WithMany(p => p.MediaItems)
                .HasForeignKey(m => m.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // PostVote -> Post
            builder.Entity<PostVote>()
                .HasOne(v => v.Post)
                .WithMany(p => p.Votes)
                .HasForeignKey(v => v.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // PostVote -> User
            builder.Entity<PostVote>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<PostVote>()
                .HasIndex(v => new { v.PostId, v.UserId })
                .IsUnique();
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var updatedPosts = ChangeTracker.Entries<Posts>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var post in updatedPosts)
            {
                post.Entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
