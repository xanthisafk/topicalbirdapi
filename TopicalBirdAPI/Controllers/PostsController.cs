using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Data.DTO.PostDTO;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IWebHostEnvironment _env;

        public PostsController(AppDbContext context, UserManager<Users> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // POST: api/Posts/new
        [Authorize]
        [HttpPost("new")]
        [RequestSizeLimit(30 * 1024 * 1024)] // 30 MB
        public async Task<IActionResult> CreatePost([FromForm] CreatePostRequest request)
        {
            // get currently logged in user
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null || !Guid.TryParse(userIdString, out var authorId))
            {
                return Unauthorized("User is not authenticated or user ID is missing.");
            }

            // Check image limit
            var imageCount = request.MediaItems.Count;
            if (imageCount > 10)
            {
                return BadRequest("A maximum of 10 images can be uploaded per post.");
            }

            // create the Post entity
            var post = new Posts
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                AuthorId = authorId,
                NestId = request.NestId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var mediaItems = new List<Media>();

            // File Storage and Media Entity Creation
            if (imageCount > 0)
            {
                // defines the path where media files will be saved
                var mediaUploadsFolder = Path.Combine(_env.WebRootPath, $"content/posts/{post.Id.ToString().Replace("-", "_").ToLower()}/uploads");
                Directory.CreateDirectory(mediaUploadsFolder); // Ensure directory exists

                for (int i = 0; i < imageCount; i++)
                {
                    var file = request.MediaItems[i].Image;
                    var altText = request.MediaItems[i].AltText;

                    if (!file.ContentType.StartsWith("image/"))
                    {
                        return BadRequest($"File {file.FileName} is not an image.");
                    }

                    // Create a unique file name
                    var fileExtension = Path.GetExtension(file.FileName);
                    var uniqueFileName = $"{post.Id.ToString().Replace("-", "_").ToLower()}_{i}{DateTime.Now.Ticks}{fileExtension}";
                    var filePath = Path.Combine(mediaUploadsFolder, uniqueFileName);

                    // Save the file to the file system
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    
                    var contentUrl = $"/{mediaUploadsFolder}/{uniqueFileName}";

                    // create the Media entity
                    mediaItems.Add(new Media
                    {
                        Id = Guid.NewGuid(),
                        PostId = post.Id,
                        ContentUrl = contentUrl,
                        AltText = altText ?? "Image uploaded with the post"
                    });
                }
                post.MediaItems = mediaItems;
            }

            _context.Posts.Add(post);
            _context.Media.AddRange(mediaItems);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
        }

        [HttpGet]
        public async Task<IActionResult> GetPostsByVote(bool asc = true)
        {
            var postVotesQuery = _context.PostVotes
            .GroupBy(pv => pv.PostId)
            .Select(g => new
            {
                PostId = g.Key,
                TotalVotes = g.Sum(pv => pv.VoteValue)
            });

            IOrderedQueryable<Posts> orderedPosts;

            var postsOrderedByVotes = asc
            ? postVotesQuery.OrderBy(v => v.TotalVotes)
            : postVotesQuery.OrderByDescending(v => v.TotalVotes);

            var orderedPostIds = await postsOrderedByVotes
            .Select(x => x.PostId)
            .ToListAsync();

            var posts = await _context.Posts
            .Where(p => orderedPostIds.Contains(p.Id) && !p.IsDeleted)
            .Include(p => p.Author)
            .Include(p => p.Nest)
            .Include(p => p.Votes)
            .Include(p => p.Comments)
            .Include(p => p.MediaItems)
            .ToListAsync();

            var finalOrderedPosts = posts.OrderBy(p => orderedPostIds.IndexOf(p.Id)).ToList();

            if (!finalOrderedPosts.Any())
            {
                return Ok(new List<string>());
            }

            return Ok(finalOrderedPosts);
        }

        [HttpGet("user/{userId:Guid}")]
        public async Task<IActionResult> GetPostsByUser(Guid userId)
        {
            var posts = await _context.Posts
                .Where(p => p.AuthorId == userId && !p.IsDeleted)
                .Include(p => p.Author)
                .Include(p => p.Nest)
                .Include(p => p.Votes)
                .Include(p => p.Comments)
                .Include(p => p.MediaItems)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            if (!posts.Any())
            {
                return Ok(new List<string>());
            }

            return Ok(posts);
        }

        [HttpGet("nest/{nestId:Guid}")]
        public async Task<IActionResult> GetPostsByNest(Guid nestId)
        {
            var posts = await _context.Posts
                .Where(p => p.NestId == nestId && !p.IsDeleted)
                .Include(p => p.Author)
                .Include(p => p.Nest)
                .Include(p => p.Votes)
                .Include(p => p.Comments)
                .Include(p => p.MediaItems)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            if (!posts.Any())
            {
                return Ok(new List<string>());
            }

            return Ok(posts);
        }

        // Example Get method (you'd need to implement this fully)
        [HttpGet("{id}")]
        public async Task<ActionResult<Posts>> GetPostById(Guid id)
        {
            var post = await _context.Posts
                .Include(p => p.MediaItems) // Make sure to include media when fetching the post
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

    }
}
