using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Data.Constants;
using TopicalBirdAPI.Data.DTO.NestDTO;
using TopicalBirdAPI.Helpers;
using TopicalBirdAPI.Migrations;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NestController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<NestController> _logger;

        public NestController(AppDbContext context, UserManager<Users> userManager, ILogger<NestController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("search/{query}")]
        public async Task<IActionResult> SearchByQuery(string query, [FromQuery] int pageNo = 1, [FromQuery] int limit = 20)
        {
            string searchQuery = query.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return BadRequest(new { message = ErrorMessages.SearchNoQuery });
            }

            if (searchQuery.Length < 3)
            {
                return BadRequest(new { message = ErrorMessages.QueryTooSmall });
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
                .Where(n => n.Title.ToLower().Contains(searchQuery) || n.Description.ToLower().Contains(searchQuery));

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
                .Select(n => NestResponse.FromNest(n, false, false))
                .ToListAsync();

            return Ok(new
            {
                pagination = new
                {
                    PageNumber = pageNo,
                    Limit = limit,
                    TotalItems = totalCount,
                    TotalPages = totalPages
                },
                nests
            });
        }

        [HttpGet]
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
            .Select(n => NestResponse.FromNest(n, false, false))
            .ToListAsync();

            return Ok(new
            {
                pagination = new
                {
                    PageNumber = pageNo,
                    Limit = limit,
                    TotalItems = totalCount,
                    TotalPages = totalPages
                },
                nests
            });
        }


        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyNests(bool withPosts = false)
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorMessages.UnauthorizedAction);
            }

            var nests = await _context.Nests
                .Include(n => n.Moderator)
                .Include(n => n.Posts)
                .Where(n => n.ModeratorId == currentUser.Id)
                .Select(n => NestResponse.FromNest(n, withPosts, currentUser.IsAdmin))
                .ToListAsync();

            return Ok(new { nests });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetSingleNestById(Guid id)
        {
            var nest = await _context.Nests
                .Include(n => n.Moderator)
                .Include(n => n.Posts)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nest == null)
            {
                return NotFound(new { message = ErrorMessages.NestNotFound });
            }

            return Ok(new { nest = NestResponse.FromNest(nest, true) });
        }

        [HttpGet("title/{title}")]
        public async Task<IActionResult> GetSingleNestByTitle(string title)
        {
            var nest = await _context.Nests
                .Include(n => n.Moderator)
                .Include(n => n.Posts)
                .FirstOrDefaultAsync(n => n.Title == title);

            if (nest == null)
            {
                return NotFound(new { message = ErrorMessages.NestNotFound });
            }

            return Ok(new { nest = NestResponse.FromNest(nest) });
        }


        [HttpPost("new")]
        [Authorize]
        public async Task<IActionResult> CreateSingleNest([FromForm] CreateNestRequest nestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorMessages.UnauthorizedAction);
            }

            if ((await _context.Nests.FirstOrDefaultAsync(n => n.Title == nestDto.Title)) != null)
            {
                return Conflict(new { message = ErrorMessages.NestTitleConflict });
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
                    var temp = await FileUploadHelper.SaveFile(nestDto.Icon, nestFolder, "icon");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        iconPath = temp;
                    }
                }
                newNest.Icon = iconPath;

                _context.Nests.Add(newNest);
                await _context.SaveChangesAsync();
                return Ok(new { message = SuccessMessages.NestCreated, nest = NestResponse.FromNest(newNest, false, currentUser.IsAdmin) });
            }
            catch (InvalidDataException idx)
            {
                _logger.LogWarning("Failed to create nest. User Error. Errors: {@Errors}", idx.Message);
                return BadRequest(new { message = idx.Message });
            }
            catch (Exception ex)
            {
                if (iconPath != "/content/assets/defaults/nest_256.png")
                {
                    FileUploadHelper.DeleteFile(iconPath);
                }
                _logger.LogCritical("Failed to create nest. Errors: {@Errors}", ex.Message);
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }

        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateNest(Guid id, [FromForm] UpdateNestRequest newNest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorMessages.UnauthorizedAction);
            }

            var nest = await _context.Nests.Include(n => n.Moderator).FirstOrDefaultAsync(n => n.Id == id);
            if (nest == null)
            {
                return NotFound(ErrorMessages.NestNotFound);
            }

            if (currentUser.Id != nest.ModeratorId && !currentUser.IsAdmin)
            {
                return Forbid(ErrorMessages.ForbiddenAction);
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
                    var temp = await FileUploadHelper.SaveFile(newNest.Icon, nestFolder, "icon");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        iconPath = temp;
                    }
                }
                nest.Icon = iconPath;

                _context.Nests.Update(nest);
                await _context.SaveChangesAsync();
                return Ok(new { message = SuccessMessages.NestUpdated, nest = NestResponse.FromNest(nest, false, currentUser.IsAdmin) });
            }
            catch (InvalidDataException idx)
            {
                _logger.LogWarning("Failed to create nest. Errors: {@Errors}", idx.Message);
                return BadRequest(new { message = idx.Message });
            }
            catch (Exception ex)
            {
                if (iconPath != originalPath)
                {
                    FileUploadHelper.DeleteFile(iconPath);
                }
                _logger.LogCritical("Failed to create nest. Errors: {@Errors}", ex.Message);
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }
    }
}
