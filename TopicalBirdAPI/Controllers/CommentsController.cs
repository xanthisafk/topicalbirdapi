using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Data.API;
using TopicalBirdAPI.Data.Constants;
using TopicalBirdAPI.Data.DTO.CommentDTO;
using TopicalBirdAPI.Helpers;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Controllers
{
    /// <summary>
    /// Post comment manipulation
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CommentsController : ControllerBase
    {
        #region Constructor
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly LoggingHelper _logger;

        public CommentsController(AppDbContext context, UserManager<Users> userManager, LoggingHelper logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }
        #endregion

        #region CREATE Opertaion
        /// <summary>
        /// Create a new comment
        /// </summary>
        /// <param name="postId">Post ID of post to comment on</param>
        /// <param name="comment">DTO of comment</param>
        /// <returns>Created comment</returns>
        /// <response code="200">Returns created comment.</response>
        /// <response code="400">If provided data is invalid.</response>
        /// <response code="401">If user is not logged in.</response>
        /// <response code="404">If post is not found.</response>
        [HttpPost("add/{postId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<CommentResponse>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> CreateComment(Guid postId, [FromForm] CreateCommentRequest comment)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.CreateFromModelState(ModelState));
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null) return Unauthorized(ErrorResponse.Create(ErrorMessages.UnauthorizedAction));

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.PostNotFound));
            }

            if (string.IsNullOrEmpty(comment.Content))
            {
                return BadRequest(ErrorResponse.Create(ErrorMessages.CommentNotFound));
            }

            Comment cmt = new Comment
            {
                AuthorId = currentUser.Id,
                Content = comment.Content,
                PostId = postId,
                CreatedAt = DateTime.UtcNow

            };

            _context.Comments.Add(cmt);
            await _context.SaveChangesAsync();
            _logger.Info($"Created a new comment: {cmt.Id}, Post: {cmt.PostId}");
            await _context.Entry(cmt).Reference(c => c.Author).LoadAsync();

            var response = CommentResponse.FromComment(cmt);
            return Ok(SuccessResponse<CommentResponse>.Create(SuccessMessages.CommentCreated, response));
        }
        #endregion

        #region READ Operation
        /// <summary>
        /// Get all comment of post
        /// </summary>
        /// <param name="postId">The post ID to fetch</param>
        /// <returns>Object containing all comments</returns>
        /// <response code="200">Returns the list of comments</response>
        /// <response code="404">If the post is not found.</response>
        [HttpGet("get/{postId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<List<CommentResponse>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> GetAllCommentsOfPostById(Guid postId)
        {
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.PostNotFound));
            }

            var comments = await _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .Include(c => c.Author)
                .Select(c => CommentResponse.FromComment(c))
                .ToListAsync();

            return Ok(SuccessResponse<List<CommentResponse>>.Create("",comments));
        }

        #endregion

        #region UPDATE Operations
        /// <summary>
        /// Update a comment
        /// </summary>
        /// <param name="commentId">Id of comment to update</param>
        /// <param name="commentDto">Comment data</param>
        /// <returns>updated comment.</returns>
        /// <response code="200">Returns the updated comment.</response>
        /// <response code="400">If provided data is incorrect.</response>
        /// <response code="403">If user is not the comment author.</response>
        /// <response code="404">If comment was not found.</response>
        [HttpPatch("edit/{commentId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(SuccessResponse<CommentResponse>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> UpdateCommentContent(Guid commentId, [FromForm] CreateCommentRequest commentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.CreateFromModelState(ModelState));
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null) return Unauthorized(ErrorResponse.Create(ErrorMessages.UnauthorizedAction));


            var cmt = await _context.Comments.FindAsync(commentId);

            if (cmt == null || cmt.IsDeleted)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.CommentNotFound));
            }
            if (cmt.AuthorId != currentUser.Id)
            {
                return StatusCode(403, ErrorResponse.Create(ErrorMessages.ForbiddenAction));
            }

            cmt.Content = commentDto.Content;
            _context.Comments.Update(cmt);
            await _context.SaveChangesAsync();

            _logger.Info("Updated comment: {cmt.Id}, author: {cmt.AuthorId} by user: {currentUser.Id}.");

            await _context.Entry(cmt).Reference(c => c.Author).LoadAsync();
            var response = CommentResponse.FromComment(cmt);
            return Ok(SuccessResponse<CommentResponse>.Create(SuccessMessages.CommentCreated, response));
        }
        #endregion

        #region DELETE Operations
        /// <summary>
        /// Deletes a comment
        /// </summary>
        /// <param name="commentId">Id of comment to delete</param>
        /// <returns>Success message</returns>
        /// <response code="404">If comment is not found</response>
        /// <response code="403">If user isnt author or admin</response>
        /// <response code="401">If user int logged in</response>
        /// <response code="200">Success message</response>
        [HttpDelete("remove/{commentId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> RemoveCommentById(Guid commentId)
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null) return Unauthorized(ErrorResponse.Create(ErrorMessages.UnauthorizedAction));

            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null || comment.IsDeleted)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.CommentNotFound));
            }
            if (comment.AuthorId != currentUser.Id && !currentUser.IsAdmin)
            {
                return StatusCode(403, ErrorResponse.Create(ErrorMessages.ForbiddenAction));
            }

            comment.IsDeleted = true;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return Ok(SuccessResponse<string>.Create(SuccessMessages.CommentDeleted, null));
        }
        #endregion
    }
}
