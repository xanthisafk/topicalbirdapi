using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopicalBirdAPI.Constants;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.DTO;
using TopicalBirdAPI.Helpers;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public CommentsController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpGet("get/{postId:guid}")]
        public async Task<IActionResult> GetAllCommentsOfPostById(Guid postId)
        {
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
            {
                return NotFound(new { message = ErrorMessages.PostNotFound });
            }

            var comments = await _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .Include(c => c.Author)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    Author = new
                    {
                        c.Author!.Id,
                        c.Author.Icon,
                        c.Author.UserName,
                        c.Author.IsAdmin
                    }
                })
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPost("add/{postId:guid}")]
        [Authorize]
        public async Task<IActionResult> CreateComment(Guid postId, [FromForm] CreateCommentRequest comment)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (postId != comment.PostId)
            {
                return BadRequest(new { message = ErrorMessages.PostNotFound });
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null) return Unauthorized(new { message = ErrorMessages.UnauthorizedAction });

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound(new { message = ErrorMessages.PostNotFound });
            }

            if (string.IsNullOrEmpty(comment.Content))
            {
                return BadRequest(new { message = ErrorMessages.CommentNotFound });
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
            await _context.Entry(cmt).Reference(c => c.Author).LoadAsync();

            var response = CommentResponse.FromComment(cmt);
            return Ok(new { message = SuccessMessages.CommentCreated, comment = response });
        }

        [HttpPut("edit/{commentId:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateCommentContent(Guid commentId, [FromForm] CreateCommentRequest commentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null) return Unauthorized(new { message = ErrorMessages.UnauthorizedAction });


            var cmt = await _context.Comments.FindAsync(commentId);

            if (cmt == null || cmt.IsDeleted)
            {
                return NotFound(new { message = ErrorMessages.CommentNotFound });
            }
            if (cmt.AuthorId != currentUser.Id)
            {
                return Forbid(ErrorMessages.ForbiddenAction);
            }

            cmt.Content = commentDto.Content;
            _context.Comments.Update(cmt);
            await _context.SaveChangesAsync();
            await _context.Entry(cmt).Reference(c => c.Author).LoadAsync();
            var response = CommentResponse.FromComment(cmt);
            return Ok(new { message = SuccessMessages.CommentUpdated, comment = response });
        }


        // DELETE api/<CommentsController>/remove/5
        [HttpDelete("remove/{commentId:guid}")]
        [Authorize]
        public async Task<IActionResult> RemoveCommentById(Guid commentId)
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null) return Unauthorized(new { message = ErrorMessages.UnauthorizedAction });

            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null || comment.IsDeleted)
            {
                return NotFound(new { message = ErrorMessages.CommentNotFound });
            }
            if (comment.AuthorId != currentUser.Id && !currentUser.IsAdmin)
            {
                return Forbid(ErrorMessages.ForbiddenAction);
            }

            comment.IsDeleted = true;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return Ok(new {message = SuccessMessages.CommentDeleted});
        }
    }
}
