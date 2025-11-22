using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Data.API;
using TopicalBirdAPI.Data.Constants;
using TopicalBirdAPI.Data.DTO.NestDTO;
using TopicalBirdAPI.Helpers;
using TopicalBirdAPI.Interface;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Controllers
{
    /// <summary>
    /// Controls nest related actions
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class NestController : ControllerBase
    {
        #region Constructor
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly LoggingHelper _logger;
        private readonly IFileHandler _files;

        public NestController(AppDbContext context, UserManager<Users> userManager, LoggingHelper logger, IFileHandler files)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _files = files;
        }
        #endregion

        #region CREATE Operations
        /// <summary>
        /// Create a new nest
        /// </summary>
        /// <param name="nestDto">DTO for creating new nest</param>
        /// <response code="200">Returns newly created nest</response>
        /// <response code="400">If provided data is invalid.</response>
        /// <response code="401">If user is not logged in</response>
        /// <response code="409">If nest already exists.</response>
        /// <response code="500">If an unexpected error happens.</response>
        [HttpPost("new")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<NestResponse>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> CreateSingleNest([FromForm] CreateNestRequest nestDto)
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

            if ((await _context.Nests.FirstOrDefaultAsync(n => n.Title == nestDto.Title)) != null)
            {
                return Conflict(ErrorResponse.Create(ErrorMessages.NestTitleConflict));
            }

            Nest newNest = new Nest
            {
                Id = Guid.NewGuid(),
                Title = nestDto.Title,
                Description = nestDto.Description ?? "",
                DisplayName = nestDto.DisplayName ?? nestDto.Title,
                CreatedAt = DateTime.UtcNow,
                Moderator = currentUser,
                ModeratorId = currentUser.Id,
            };
            string iconPath = "/content/assets/defaults/nest_256.png";
            try
            {

                if (nestDto.Icon != null)
                {
                    var nestFolder = Path.Combine("wwwroot/content/uploads/nests", newNest.Id.ToString().ToLower().Replace("-", "_"));
                    var temp = await _files.SaveFile(nestDto.Icon, nestFolder, "icon");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        iconPath = temp;
                    }
                }
                newNest.Icon = iconPath;

                _context.Nests.Add(newNest);
                await _context.SaveChangesAsync();
                _logger.Info($"Created new nest: {newNest.Title}, id: {newNest.Id}");
                ErrorResponse.Create(ErrorMessages.UnauthorizedAction);
                return Ok(SuccessResponse<NestResponse>.Create(SuccessMessages.NestCreated, NestResponse.FromNest(newNest, currentUser.IsAdmin)));
            }
            catch (InvalidDataException idx)
            {
                string refCode = await _logger.Error("Failed to create nest.", idx);
                return BadRequest();
            }
            catch (Exception ex)
            {
                if (iconPath != "/content/assets/defaults/nest_256.png")
                {
                    _files.DeleteFile(iconPath);
                }
                string refCode = await _logger.Crit("Failed to create nest", ex);
                return StatusCode(500, ErrorResponse.Create(ErrorMessages.InternalServerError, null, refCode));
            }
        }
        #endregion

        #region READ Operations

        /// <summary>
        /// Get nest by id
        /// </summary>
        /// <param name="id">Id of nest</param>
        /// <response code="200">Returns the nest</response>
        /// <response code="404">If nest is not found.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<NestResponse>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> GetSingleNestById(Guid id)
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            var nest = await _context.Nests
                .Include(n => n.Moderator)
                .Include(n => n.Posts)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nest == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.NestNotFound));
            }

            return Ok(SuccessResponse<NestResponse>.Create("", NestResponse.FromNest(nest, currentUser != null && currentUser.IsAdmin)));
        }


        /// <summary>
        /// Get nest by nest title
        /// </summary>
        /// <param name="title">Title of nest</param>
        /// <response code="200">Returns the nest</response>
        /// <response code="404">If nest is not found.</response>
        [HttpGet("title/{title}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<NestResponse>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> GetSingleNestByTitle(string title)
        {
            var nest = await _context.Nests
                .Include(n => n.Moderator)
                .Include(n => n.Posts)
                .FirstOrDefaultAsync(n => n.Title == title);

            if (nest == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.NestNotFound));
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            return Ok(SuccessResponse<NestResponse>.Create("", NestResponse.FromNest(nest, currentUser != null && currentUser.IsAdmin)));
        }


        /// <summary>
        /// Search a nest
        /// </summary>
        /// <param name="query">Text to search</param>
        /// <param name="pageNo">Page no. to get</param>
        /// <param name="limit">Amount of results to get</param>
        /// <response code="200">Returns pagination and list of nests</response>
        /// <response code="400">If the provided data is incorrect.</response>
        [HttpGet("search/{query}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<object>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> SearchByQuery(string query, [FromQuery] int pageNo = 1, [FromQuery] int limit = 20)
        {
            string searchQuery = query.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return BadRequest(ErrorResponse.Create(ErrorMessages.SearchNoQuery));
            }

            if (searchQuery.Length < 3)
            {
                return BadRequest(ErrorResponse.Create(ErrorMessages.QueryTooSmall));
            }

            if (pageNo < 1)
            {
                pageNo = 1;
            }

            if (limit < 1 || limit > 50)
            {
                limit = 20;
            }

            int skipCount = (pageNo - 1) * limit;

            var baseQuery = _context.Nests
                .Where(n => n.Title.ToLower().Contains(searchQuery) || (n.Description != null && n.Description.ToLower().Contains(searchQuery)));

            int totalCount = await baseQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)limit);

            if (pageNo > totalPages && totalPages > 0)
            {
                pageNo = totalPages;
            }

            var nests = await baseQuery
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skipCount)
                .Take(limit)
                .Include(n => n.Moderator)
                .Include(n => n.Posts)
                .Select(n => NestResponse.FromNest(n, false))
                .ToListAsync();

            return Ok(

                SuccessResponse<object?>.Create(null, new
                {
                    pagination = new
                    {
                        PageNumber = pageNo,
                        Limit = limit,
                        TotalItems = totalCount,
                        TotalPages = totalPages
                    },
                    nests
                })
            );
        }

        /// <summary>
        /// Get all nests
        /// </summary>
        /// <param name="pageNo">Search page number</param>
        /// <param name="limit">Amount of results to return</param>
        /// <response code="200">Returns pagination and list of nests</response>
        /// <response code="400">If the provided data is incorrect.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<object>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> GetPaginatedNests(int pageNo = 1, int limit = 20)
        {
            if (pageNo < 1)
            {
                pageNo = 1;
            }

            if (limit < 1 || limit > 50)
            {
                limit = 20;
            }

            int skipCount = (pageNo - 1) * limit;

            int totalCount = await _context.Nests.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)limit);

            var query = _context.Nests
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skipCount)
                .Take(limit)
                .Include(n => n.Moderator);

            var nests = await query
            .Select(n => NestResponse.FromNest(n, false))
            .ToListAsync();

            return Ok(

                SuccessResponse<object>.Create(null, new
                {
                    pagination = new
                    {
                        PageNumber = pageNo,
                        Limit = limit,
                        TotalItems = totalCount,
                        TotalPages = totalPages
                    },
                    nests
                })
            );
        }


        /// <summary>
        /// Get nests moderated by authenticated user
        /// </summary>
        /// <response code="200">Returns list of nests moderated by user</response>
        /// <response code="401">The user is not logged in.</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<List<NestResponse>>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> GetMyNests()
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorResponse.Create(ErrorMessages.UnauthorizedAction));
            }

            var nests = await _context.Nests
                .Include(n => n.Moderator)
                .Include(n => n.Posts)
                .Where(n => n.ModeratorId == currentUser.Id)
                .Select(n => NestResponse.FromNest(n, currentUser.IsAdmin))
                .ToListAsync();

            return Ok(SuccessResponse<List<NestResponse>>.Create("", nests));
        }

        /// <summary>
        /// Get nests by username
        /// </summary>
        /// <param name="handle">Username of user</param>
        [HttpGet("user/{handle}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<List<NestResponse>>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> GetNestsByUsername(string handle)
        {
            var user = await _context.Users.Include(u => u.Nests).FirstOrDefaultAsync(u => u.Handle == handle);
            if (user == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.UserNotFound, null, null));
            }
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (user.Nests == null)
            {
                return Ok(new SuccessResponse<List<NestResponse>>());
            }
            var response = user.Nests.Select(n => NestResponse.FromNest(n, currentUser?.IsAdmin ?? false)).ToList();
            return Ok(SuccessResponse<List<NestResponse>>.Create(null, response));
        }

        /// <summary>
        /// Get nests by Id
        /// </summary>
        /// <param name="id">Id of user</param>
        [HttpGet("user/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<List<NestResponse>>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> GetNestsByUserId(Guid id)
        {
            var user = await _context.Users.Include(u => u.Nests).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.UserNotFound, null, null));
            }
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (user.Nests == null)
            {
                return Ok(new SuccessResponse<List<NestResponse>>());
            }
            var response = user.Nests.Select(n => NestResponse.FromNest(n, currentUser?.IsAdmin ?? false)).ToList();
            return Ok(SuccessResponse<List<NestResponse>>.Create(null, response));
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Updates nest data
        /// </summary>
        /// <param name="id">Id of nest to update</param>
        /// <param name="newNest">DTO used to retrieve data in a contained way.</param>
        /// <response code="200">Returns udpated nest</response>
        /// <response code="400">If provided data is invalid.</response>
        /// <response code="401">If user is not logged in</response>
        /// <response code="500">If an unexpected error happens.</response>
        [HttpPatch("{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<NestResponse>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> UpdateNest(Guid id, [FromForm] UpdateNestRequest newNest)
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

            var nest = await _context.Nests.Include(n => n.Moderator).FirstOrDefaultAsync(n => n.Id == id);
            if (nest == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.NestNotFound));
            }

            if (currentUser.Id != nest.ModeratorId && !currentUser.IsAdmin)
            {
                return StatusCode(403, ErrorResponse.Create(ErrorMessages.ForbiddenAction));
            }

            nest.Description = newNest.Description ?? nest.Description;
            nest.DisplayName = newNest.DisplayName ?? nest.DisplayName;

            string iconPath = nest.Icon;
            string originalPath = nest.Icon;
            try
            {
                if (newNest.Icon != null)
                {
                    var nestFolder = Path.Combine("wwwroot/content/uploads/nests", nest.Id.ToString().ToLower().Replace("-", "_"));
                    var temp = await _files.SaveFile(newNest.Icon, nestFolder, "icon");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        iconPath = temp;
                    }
                }
                nest.Icon = iconPath;

                _context.Nests.Update(nest);
                await _context.SaveChangesAsync();
                _logger.Info($"Updated nest: {nest.Id} by {currentUser.Id}");
                return Ok(SuccessResponse<NestResponse>.Create(SuccessMessages.NestUpdated, NestResponse.FromNest(nest, currentUser.IsAdmin)));
            }
            catch (InvalidDataException idx)
            {
                string refCode = await _logger.Error("Failed to create nest.", idx);
                return BadRequest(ErrorResponse.Create(ErrorMessages.InvalidRequest, idx.Message, refCode));
            }
            catch (Exception ex)
            {
                if (iconPath != originalPath)
                {
                    _files.DeleteFile(iconPath);
                }
                string refCode = await _logger.Crit("Failed to create nest.", ex);
                return StatusCode(500, ErrorResponse.Create(ErrorMessages.InternalServerError, null, refCode));
            }
        }
        #endregion
    }
}
