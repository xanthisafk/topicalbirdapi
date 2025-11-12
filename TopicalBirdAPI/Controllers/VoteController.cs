using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Data.API;
using TopicalBirdAPI.Data.Constants;
using TopicalBirdAPI.Data.DTO.PostDTO;
using TopicalBirdAPI.Data.DTO.VoteDTO;
using TopicalBirdAPI.Helpers;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Controllers
{
    /// <summary>
    /// Controls votes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VoteController : Controller
    {
        #region Constructor

        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly LoggingHelper _logger;
        /// <summary>
        /// Constructor
        /// </summary>
        public VoteController(AppDbContext context, UserManager<Users> userManager, LoggingHelper logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        #endregion

        #region CREATE Operations

        /// <summary>
        /// Casts a vote
        /// </summary>
        /// <param name="postId">Post that is getting the vote</param>
        /// <param name="dto">Helper class for getting value from client</param>
        /// <response code="200">Returns vote info. If delete, returns null.</response>
        /// <response code="400">If provided data is invalid.</response>
        /// <response code="401">If user is not logged in.</response>
        /// <response code="404">If post is not found.</response>
        /// <response code="409">If vote has not changed.</response>
        [HttpPost("{postId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<VoteResponse>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> CastVote(Guid postId, [FromBody] CreateVoteRequest dto)
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

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.PostNotFound));
            }

            string responseMessage = ErrorMessages.InternalServerError;
            var vote = await _context.PostVotes.FirstOrDefaultAsync(v => v.PostId == postId && v.UserId == currentUser.Id);
            if (vote != null && vote.VoteValue == dto.VoteValue)
            {
                return Conflict(ErrorResponse.Create(ErrorMessages.VoteAlreadyExists));
            }
            else if (vote == null && dto.VoteValue == 0)
            {
                return BadRequest(ErrorMessages.VoteNotCast);
            }
            else if (vote != null && vote.VoteValue != dto.VoteValue)
            {
                if (dto.VoteValue == 0)
                {
                    _context.PostVotes.Remove(vote);
                    responseMessage = SuccessMessages.VoteRemoved;
                }
                else
                {
                    vote.VoteValue = dto.VoteValue;
                    vote.UpdatedAt = DateTime.UtcNow;
                    _context.PostVotes.Update(vote);
                    responseMessage = SuccessMessages.VoteUpdated;
                }
            }
            else if (vote == null && dto.VoteValue != 0)
            {
                vote = new PostVote
                {
                    VoteValue = dto.VoteValue,
                    PostId = postId,
                    UserId = currentUser.Id,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.PostVotes.Add(vote);
                responseMessage = SuccessMessages.VoteAdded;
            }
            else
            {
                return StatusCode(500, ErrorResponse.Create(ErrorMessages.InternalServerError, null, null));
            }

            await _context.SaveChangesAsync();
            _logger.Info($"User {currentUser.Handle} casted {dto.VoteValue} vote on {post.Id}");
            return Ok(SuccessResponse<string>.Create(responseMessage, responseMessage));
        }

        #endregion

        #region READ Operations

        /// <summary>
        /// Gets vote score of a post
        /// </summary>
        /// <param name="postId">Id of post to check vote of</param>
        /// <response code="200">Returns the score of post,</response>
        /// <response code="404">If post is not found</response>
        [HttpGet("{postId:guid}/score")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<object>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> GetScore(Guid postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.PostNotFound));
            }

            var totalVotes = _context.PostVotes.Where(v => v.PostId == postId).Sum(v => v.VoteValue);
            return Ok(SuccessResponse<object>.Create("", new
            {
                Score = totalVotes,
                Post = PostResponse.FromPost(post)
            }));
        }

        #endregion
    }
}
