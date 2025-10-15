using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Data.Constants;
using TopicalBirdAPI.Data.DTO.PostDTO;
using TopicalBirdAPI.Helpers;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        #region Constructor
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<PostsController> _logger;

        public PostsController(AppDbContext context, UserManager<Users> userManager, ILogger<PostsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }
        #endregion

        #region CREATE Operations
        [HttpPost("new")]
        [Authorize]
        public async Task<IActionResult> CreateNewPost([FromForm] CreatePostRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(new { message = ErrorMessages.UnauthorizedAction });
            }

            // Eager load Nest, including its Moderator to prevent DTO NRE
            var nest = await _context.Nests
                .Include(n => n.Moderator)
                .FirstOrDefaultAsync(n => n.Title == dto.NestTitle);
            if (nest == null)
            {
                return NotFound(new { message = ErrorMessages.NestNotFound });
            }

            var post = new Posts
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Content = dto.Content,
                Author = currentUser,
                Nest = nest,
                CreatedAt = DateTime.UtcNow,
                Votes = new List<PostVote>(), // Use new List<T>() for clarity
                Comments = new List<Comment>(),
            };

            var media = dto.MediaItems();
            List<Media> mediaItems = new List<Media>();
            string postsFolder = string.Empty;

            // Use a transaction for atomicity: either DB and files save, or nothing does.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Save post to DB first (or prepare to)
                await _context.Posts.AddAsync(post);
                await _context.SaveChangesAsync(); // Save to get PostId, which is used for the folder name

                // 2. Upload media and update post entity
                if (media.Count > 0)
                {
                    string postGuid = post.Id.ToString().ToLower().Replace("-", "_");
                    postsFolder = Path.Combine("wwwroot/content/uploads/posts", postGuid);

                    for (int i = 0; i < media.Count; i++)
                    {
                        var item = media[i];
                        string mediaPath = await FileUploadHelper.SaveFile(media[i].Image, postsFolder, $"{postGuid}_{i}");
                        mediaItems.Add(new Media
                        {
                            PostId = post.Id,
                            ContentUrl = mediaPath,
                            AltText = media[i].AltText
                        });
                    }
                    post.MediaItems = mediaItems;
                    _context.Media.AddRange(mediaItems); // Add media entities to context
                    await _context.SaveChangesAsync(); // Save media items
                }

                // 3. Commit transaction
                await transaction.CommitAsync();

                _logger.LogInformation("New post created. Details: {@post}", post);
                return Ok(new { message = SuccessMessages.PostCreated, post = PostResponse.FromPost(post) });
            }
            catch (InvalidDataException idx)
            {
                await transaction.RollbackAsync(); // Rollback DB changes
                _logger.LogError(idx, "A user error occurred during media upload or processing. Post ID: {PostId}", post.Id);

                // Cleanup files if the folder was created/used
                if (!string.IsNullOrEmpty(postsFolder) && Directory.Exists(postsFolder))
                {
                    Directory.Delete(postsFolder, true);
                }

                return BadRequest(new { message = idx.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Rollback DB changes
                _logger.LogCritical(ex, "An unexpected error occurred during post creation. Post ID: {PostId}", post.Id);

                // Cleanup files
                if (!string.IsNullOrEmpty(postsFolder) && Directory.Exists(postsFolder))
                {
                    Directory.Delete(postsFolder, true);
                }

                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }
        #endregion

        #region READ Operations

        [HttpGet("{postId:guid}")]
        public async Task<IActionResult> GetSinglePost(Guid postId)
        {
            var currentPost = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.MediaItems)
                .Include(p => p.Votes)
                .Include(p => p.Nest)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (currentPost == null)
            {
                return NotFound(new { message = ErrorMessages.PostNotFound });
            }

            return Ok(new { post = PostResponse.FromPost(currentPost) });
        }

        [HttpGet("nest")]
        public async Task<IActionResult> GetPostsOfNestViaTitle(string nestTitle, int pageNo = 1, int limit = 20)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.MediaItems)
                .Include(p => p.Votes)
                .Include(p => p.Nest)
                .Where(p => p.Nest.Title == nestTitle);

            var result = await PaginationHelper.PaginateAsync(
                query,
                p => PostResponse.FromPost(p),
                pageNo,
                limit);

            return Ok(new { result.Pagination, Posts = result.Items });
        }

        [HttpGet("user/id/{userId:guid}")]
        public async Task<IActionResult> GetPostsOfUserViaId(Guid userId, int pageNo = 1, int limit = 20)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.MediaItems)
                .Include(p => p.Votes)
                .Include(p => p.Nest)
                .Where(p => p.Author.Id == userId);

            var result = await PaginationHelper.PaginateAsync(
                query,
                p => PostResponse.FromPost(p),
                pageNo,
                limit);

            return Ok(new { result.Pagination, Posts = result.Items });
        }

        [HttpGet("user/username")]
        public async Task<IActionResult> GetPostsOfUserViaHandle(string userHandle, int pageNo = 1, int limit = 20)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.MediaItems)
                .Include(p => p.Votes)
                .Include(p => p.Nest)
                .Where(p => string.Equals(p.Author.Handle, userHandle, StringComparison.InvariantCultureIgnoreCase));

            var result = await PaginationHelper.PaginateAsync(
                query,
                p => PostResponse.FromPost(p),
                pageNo,
                limit);

            return Ok(new { result.Pagination, Posts = result.Items });
        }

        #endregion

        #region UPDATE Operations

        [HttpPatch("update/{postId:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdatePostViaId(Guid postId, [FromForm] PostUpdateRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Note: Since you only check post.AuthorId, fetching user ID is often enough.
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(new { message = ErrorMessages.UnauthorizedAction });
            }

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound(new { message = ErrorMessages.PostNotFound });
            }

            // Use the foreign key ID for comparison (post.AuthorId)
            if (post.AuthorId != currentUser.Id)
            {
                return StatusCode(403, new { message = ErrorMessages.ForbiddenAction });
            }

            // Check if there is anything to update
            if (post.Content == dto.Content)
            {
                // Use 204 No Content for a successful request where no content is returned (nothing changed)
                return NoContent();
            }

            post.Content = dto.Content;
            post.UpdatedAt = DateTime.UtcNow;

            // Explicitly calling Update as a defensive measure per your note
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Post {PostId} updated by user {UserId}.", postId, currentUser.Id);
            return Ok(new { message = SuccessMessages.PostUpdated });
        }
        #endregion

        #region DELETE Operations
        [HttpDelete("delete/{postId:guid}")]
        [Authorize]
        public async Task<IActionResult> DeletePostViaId(Guid postId)
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(new { message = ErrorMessages.UnauthorizedAction });
            }

            // Include MediaItems to clean up files
            var currentPost = await _context.Posts
                .Include(p => p.MediaItems)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (currentPost == null)
            {
                // Return 204 No Content for idempotent delete requests when the resource is already gone
                return NoContent();
            }

            // Authorization: Must be the author OR an Admin
            if (currentUser.Id != currentPost.AuthorId && !currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = ErrorMessages.ForbiddenAction });
            }

            // File Cleanup
            if (currentPost.MediaItems?.Count > 0)
            {
                try
                {
                    string postGuid = currentPost.Id.ToString().ToLower().Replace("-", "_");
                    var postsFolder = Path.Combine("wwwroot/content/uploads/posts", postGuid);

                    // Assuming all media for a post are in one folder, delete the entire folder
                    if (Directory.Exists(postsFolder))
                    {
                        Directory.Delete(postsFolder, true); // true for recursive delete
                        _logger.LogInformation("Deleted media folder for post: {PostId}", postId);
                    }
                    else
                    {
                        // Fallback: Delete individual files if the folder structure is different
                        foreach (var item in currentPost.MediaItems)
                        {
                            FileUploadHelper.DeleteFile(item.ContentUrl);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue to delete the DB record
                    _logger.LogError(ex, "Failed to delete media files for post: {PostId}", postId);
                }
            }

            _context.Posts.Remove(currentPost);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Post {PostId} deleted by user {UserId}.", postId, currentUser.Id);
            return Ok(new { message = SuccessMessages.PostDeleted });
        }
        #endregion
    }
}