using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Data.API;
using TopicalBirdAPI.Data.Constants;
using TopicalBirdAPI.Data.DTO.PostDTO;
using TopicalBirdAPI.Helpers;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PostsController : ControllerBase
    {
        #region Constructor
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly LoggingHelper _logger;

        public PostsController(AppDbContext context, UserManager<Users> userManager, LoggingHelper logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }
        #endregion

        #region CREATE Operations
        /// <summary>
        /// Creates a new post in nest.
        /// </summary>
        /// <param name="dto">The data transfer object containing the post details and optional media files.</param>
        /// <returns>A response containing the created post details.</returns>
        /// <response code="200">Returns the newly created post.</response>
        /// <response code="400">If the model state is invalid or if media processing fails.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the specified Nest is not found.</response>
        /// <response code="500">If an unexpected server error occurs.</response>
        [HttpPost("new")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<object>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> CreateNewPost([FromForm] CreatePostRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.CreateFromModelState(ModelState));
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorResponse.Create(ErrorMessages.UnauthorizedAction));
            }

            var nest = await _context.Nests
                .Include(n => n.Moderator)
                .FirstOrDefaultAsync(n => n.Title == dto.NestTitle);
            if (nest == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.NestNotFound));
            }

            var post = new Posts
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Content = dto.Content,
                Author = currentUser,
                Nest = nest,
                CreatedAt = DateTime.UtcNow,
                Votes = new List<PostVote>(),
                Comments = new List<Comment>(),
            };

            var media = dto.MediaItems();
            List<Media> mediaItems = new List<Media>();
            string postsFolder = string.Empty;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Posts.AddAsync(post);
                await _context.SaveChangesAsync();

                if (media.Count > 0)
                {
                    string postGuid = post.Id.ToString().ToLower().Replace("-", "_");
                    postsFolder = Path.Combine("wwwroot/content/uploads/posts", postGuid);

                    for (int i = 0; i < media.Count; i++)
                    {
                        var item = media[i];
                        ArgumentNullException.ThrowIfNull(item.Image);
                        string mediaPath = await FileUploadHelper.SaveFile(item.Image, postsFolder, $"{postGuid}_{i}");
                        mediaItems.Add(new Media
                        {
                            PostId = post.Id,
                            ContentUrl = mediaPath,
                            AltText = media[i].AltText
                        });
                    }
                    post.MediaItems = mediaItems;
                    _context.Media.AddRange(mediaItems);
                    await _context.SaveChangesAsync();
                }


                await transaction.CommitAsync();

                _logger.Info($"New post created. {post.Id}");
                return Ok(SuccessResponse<PostResponse>.Create(SuccessMessages.PostCreated, PostResponse.FromPost(post, currentUser)));
            }
            catch (InvalidDataException idx)
            {
                await transaction.RollbackAsync();
                string refCode = await _logger.Error("Error uploading files", idx);

                // Cleanup files if the folder was created/used
                if (!string.IsNullOrEmpty(postsFolder) && Directory.Exists(postsFolder))
                {
                    Directory.Delete(postsFolder, true);
                }

                return BadRequest(ErrorResponse.Create(ErrorMessages.InvalidRequest, null, refCode));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                string refCode = await _logger.Crit("Error creating post", ex);

                // Cleanup files
                if (!string.IsNullOrEmpty(postsFolder) && Directory.Exists(postsFolder))
                {
                    Directory.Delete(postsFolder, true);
                }

                return BadRequest(ErrorResponse.Create(ErrorMessages.InternalServerError, null, refCode));
            }
        }
        #endregion

        #region READ Operations

        /// <summary>
        /// Get a post by ID.
        /// </summary>
        /// <param name="postId">The unique id of the post.</param>
        /// <returns>A response containing the post details.</returns>
        /// <response code="200">Returns the requested post.</response>
        /// <response code="404">If the post is not found.</response>
        [HttpGet("{postId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<object>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> GetSinglePost(Guid postId)
        {
            var currentPost = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.MediaItems)
                .Include(p => p.Votes)
                .Include(p => p.Nest)
                .FirstOrDefaultAsync(p => p.Id == postId);

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentPost == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.PostNotFound));
            }

            return Ok(SuccessResponse<PostResponse>.Create(null, PostResponse.FromPost(currentPost, currentUser)));
        }

        /// <summary>
        /// Gets all posts for Nest
        /// </summary>
        /// <param name="nestTitle">The title of the Nest.</param>
        /// <param name="pageNo">Page number to fetch. Defaults to 1.</param>
        /// <param name="limit">Amount of posts that will be returned per page. Maximum is 50. Defaults to 20.</param>
        /// <returns>A paged list of posts from the Nest.</returns>
        /// <response code="200">Returns the paged list of posts.</response>
        [HttpGet("nest")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<object>))]
        public async Task<IActionResult> GetPostsOfNestViaTitle(string nestTitle, int pageNo = 1, int limit = 20)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.MediaItems)
                .Include(p => p.Votes)
                .Include(p => p.Nest)
                .Where(p => p.Nest != null && p.Nest.Title == nestTitle);
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            var result = await PaginationHelper.PaginateAsync(
                query,
                p => PostResponse.FromPost(p, currentUser),
                pageNo,
                limit);

            return Ok(SuccessResponse<object>.Create(null, new { result.Pagination, Posts = result.Items }));
        }

        /// <summary>
        /// Gets all posts from user ID
        /// </summary>
        /// <param name="userId">The unique id of the user.</param>
        /// <param name="pageNo">Page number to fetch. Defaults to 1.</param>
        /// <param name="limit">Amount of posts that will be returned per page. Maximum is 50. Defaults to 20.</param>
        /// <returns>A paged list of posts authored by the user.</returns>
        /// <response code="200">Returns the paged list of posts.</response>
        [HttpGet("user/id/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<object>))]
        public async Task<IActionResult> GetPostsOfUserViaId(Guid userId, int pageNo = 1, int limit = 20)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.MediaItems)
                .Include(p => p.Votes)
                .Include(p => p.Nest)
                .Where(p => p.Author != null && p.Author.Id == userId);

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            var result = await PaginationHelper.PaginateAsync(
                query,
                p => PostResponse.FromPost(p, currentUser),
                pageNo,
                limit);

            return Ok(new { result.Pagination, Posts = result.Items });
        }

        /// <summary>
        /// Get all posts from username
        /// </summary>
        /// <param name="userHandle">The username of the user.</param>
        /// <param name="pageNo">Page number to fetch. Defaults to 1.</param>
        /// <param name="limit">Amount of posts that will be returned per page. Maximum is 50. Defaults to 20.</param>
        /// <returns>A paged list of posts authored by the user.</returns>
        /// <response code="200">Returns the paged list of posts.</response>
        [HttpGet("user/username")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<object>))]
        public async Task<IActionResult> GetPostsOfUserViaHandle(string userHandle, int pageNo = 1, int limit = 20)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.MediaItems)
                .Include(p => p.Votes)
                .Include(p => p.Nest)
                .Where(p => p.Author != null && p.Author.Handle.ToLower() == userHandle.ToLower());

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            var result = await PaginationHelper.PaginateAsync(
                query,
                p => PostResponse.FromPost(p, currentUser),
                pageNo,
                limit);

            return Ok(new { result.Pagination, Posts = result.Items });
        }

        /// <summary>
        /// Gets latest posts
        /// </summary>
        /// <param name="nest">Title of the Nest where posts will be fetched. Optional.</param>
        /// <param name="pageNo">Page number to fetch. Defaults to 1.</param>
        /// <param name="limit">Amount of posts that will be returned per page. Maximum is 50. Defaults to 20.</param>
        /// <returns>A paged list of the latest posts.</returns>
        /// <response code="200">Returns the paged list of posts.</response>
        [HttpGet("latest")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<object>))]
        public async Task<IActionResult> GetLatestPosts(string? nest = null, int pageNo = 1, int limit = 20)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.MediaItems)
                .Include(p => p.Votes)
                .Include(p => p.Nest)
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nest))
            {
                var nestLower = nest.ToLower();
                query = query.Where(p => p.Nest != null && p.Nest.Title.ToLower() == nestLower)
                             .OrderByDescending(p => p.CreatedAt);
            }
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            var result = await PaginationHelper.PaginateAsync(
                query,
                p => PostResponse.FromPost(p, currentUser),
                pageNo,
                limit);

            return Ok(SuccessResponse<object>.Create(null, new { result.Pagination, posts = result.Items }));
        }

        /// <summary>
        /// Get popular posts
        /// </summary>
        /// <param name="nest">Title of the Nest where posts will be fetched. Optional.</param>
        /// <param name="pageNo">Page number to fetch. Defaults to 1.</param>
        /// <param name="limit">Amount of posts that will be returned per page. Maximum is 50. Defaults to 20.</param>
        /// <returns>A paged list of the popular posts.</returns>
        /// <response code="200">Returns the paged list of posts.</response>
        [HttpGet("popular")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<object>))]
        public async Task<IActionResult> GetPopularPosts(string? nest = null, int pageNo = 1, int limit = 20)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.MediaItems)
                .Include(p => p.Votes)
                .Include(p => p.Nest)
                .OrderByDescending(p => (p.Votes != null ? p.Votes.Sum(v => v.VoteValue) : 0))
                .AsQueryable();

            if (!string.IsNullOrEmpty(nest))
            {
                var nestLower = nest.ToLower();
                query = query.Where(p => p.Nest != null && p.Nest.Title.ToLower() == nestLower)
                             .OrderByDescending(p => (p.Votes != null ? p.Votes.Sum(v => v.VoteValue) : 0));
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);

            var result = await PaginationHelper.PaginateAsync(
            query,
            p => PostResponse.FromPost(p, currentUser),
            pageNo,
            limit);

            return Ok(SuccessResponse<object>.Create(null, new { result.Pagination, posts = result.Items }));
        }


        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Updates the content of an existing post.
        /// </summary>
        /// <param name="postId">The unique id of the post to update.</param>
        /// <param name="dto">The data transfer object containing the new content.</param>
        /// <returns>A confirmation message.</returns>
        /// <response code="200">If the post was successfully updated.</response>
        /// <response code="204">If the post content was the same and no update was performed.</response>
        /// <response code="400">If the model state is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the authenticated user is not the author of the post.</response>
        /// <response code="404">If the post is not found.</response>
        [HttpPatch("update/{postId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<PostResponse>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> UpdatePostViaId(Guid postId, [FromForm] PostUpdateRequest dto)
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

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound(new { message = ErrorMessages.PostNotFound });
            }

            if (post.AuthorId != currentUser.Id)
            {
                return StatusCode(403, new { message = ErrorMessages.ForbiddenAction });
            }

            if (post.Content == dto.Content || string.IsNullOrWhiteSpace(dto.Content))
            {
                return NoContent();
            }

            post.Content = dto.Content;
            post.UpdatedAt = DateTime.UtcNow;

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
            _logger.Info($"Post {postId} updated by user {currentUser.Id}.");
            return Ok(SuccessResponse<PostResponse>.Create(SuccessMessages.PostUpdated, PostResponse.FromPost(post, currentUser)));
        }
        #endregion

        #region DELETE Operations
        /// <summary>
        /// Deletes a post by its ID.
        /// </summary>
        /// <param name="postId">The unique identifier (GUID) of the post to delete.</param>
        /// <returns>A confirmation message or a 204 if the post was already deleted.</returns>
        /// <response code="200">If the post was successfully deleted.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the authenticated user is not the author or an Admin.</response>
        /// <response code="500">If a file cleanup or other server error occurs.</response>
        [HttpDelete("delete/{postId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> DeletePostViaId(Guid postId)
        {
            // get user
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(new { message = ErrorMessages.UnauthorizedAction });
            }

            // get post with media
            var currentPost = await _context.Posts
                .Include(p => p.MediaItems)
                .FirstOrDefaultAsync(p => p.Id == postId);

            // post is empty
            if (currentPost == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.PostNotFound, null, null));
            }

            // user is author or admin
            if (currentUser.Id != currentPost.AuthorId && !currentUser.IsAdmin)
            {
                return StatusCode(403, ErrorResponse.Create(ErrorMessages.ForbiddenAction, null, null));
            }

            // clean media
            if (currentPost.MediaItems?.Count > 0)
            {
                try
                {
                    string postGuid = currentPost.Id.ToString().ToLower().Replace("-", "_");
                    var postsFolder = Path.Combine("wwwroot/content/uploads/posts", postGuid);

                    if (Directory.Exists(postsFolder))
                    {
                        Directory.Delete(postsFolder, true);
                        _logger.Info($"Deleted media folder for post: {postId}");
                    }
                    else
                    {
                        foreach (var item in currentPost.MediaItems)
                        {
                            if (!string.IsNullOrEmpty(item.ContentUrl))
                                FileUploadHelper.DeleteFile(item.ContentUrl);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string refCode = await _logger.Crit($"Failed to delete media files for post: {postId}", ex);
                    return StatusCode(500, ErrorResponse.Create(ErrorMessages.InternalServerError, null, refCode));
                }
            }

            _context.Posts.Remove(currentPost);
            await _context.SaveChangesAsync();

            _logger.Info($"Post {postId} deleted by user {currentUser.Id}.");
            return Ok(SuccessResponse<string>.Create(SuccessMessages.PostDeleted, null));
        }
        #endregion
    }
}