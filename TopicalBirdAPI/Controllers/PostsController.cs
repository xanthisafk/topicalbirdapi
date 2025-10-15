using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Data.Constants;
using TopicalBirdAPI.Data.DTO.PostDTO;
using TopicalBirdAPI.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TopicalBirdAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<PostsController> _logger;

        public PostsController(AppDbContext context, UserManager<Users> userManager, ILogger<PostsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("nest")]
        public async Task<IActionResult> GetPostsOfNestViaTitle(string nestTitle, int pageNo = 1, int limit = 20)
        {
            nestTitle = nestTitle.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(nestTitle))
            {
                return BadRequest(new { message = ErrorMessages.SearchNoQuery });
            }

            if (nestTitle.Length < 3)
            {
                return BadRequest(new { message = ErrorMessages.QueryTooSmall });
            }

            pageNo = (pageNo < 1) ? 1 : pageNo;
            limit = (limit <1 || limit > 50) ? 20 : limit;
            int skipCount = (pageNo - 1) * limit;

            var query = _context.Posts.Where(p => p.Title.ToLower() == nestTitle);

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)limit);
            pageNo = (pageNo > totalPages && totalPages > 0) ? totalPages : pageNo;
            
            var posts = await query
                           .Skip(skipCount)
                           .Take(limit)
                           .Include(p => p.Author)
                           .Include(p => p.Comments)
                           .Include(p => p.MediaItems)
                           .Include(p => p.Votes)
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
                posts
            });
        }

        [HttpGet("user/id/{userId:guid}")]
        public async Task<IActionResult> GetPostsOfUserViaId(Guid userId, int pageNo = 1, int limit = 20)
        {
            pageNo = (pageNo < 1) ? 1 : pageNo;
            limit = (limit < 1 || limit > 50) ? 20 : limit;
            int skipCount = (pageNo - 1) * limit;

            var query = _context.Posts.Include(p => p.Author).Where(p => p.Author.Id == userId);

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)limit);
            pageNo = (pageNo > totalPages && totalPages > 0) ? totalPages : pageNo;

            var posts = await query
                           .Skip(skipCount)
                           .Take(limit)
                           .Include(p => p.Author)
                           .Include(p => p.Comments)
                           .Include(p => p.MediaItems)
                           .Include(p => p.Votes)
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
                posts
            });
        }

        [HttpGet("user/username")]
        public async Task<IActionResult> GetPostsOfUserViaHandle(string userHandle, int pageNo = 1, int limit = 20)
        {
            pageNo = (pageNo < 1) ? 1 : pageNo;
            limit = (limit < 1 || limit > 50) ? 20 : limit;
            int skipCount = (pageNo - 1) * limit;

            var query = _context.Posts.Include(p => p.Author).Where(p => p.Author.Handle == userHandle);

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)limit);
            pageNo = (pageNo > totalPages && totalPages > 0) ? totalPages : pageNo;

            var posts = await query
                           .Skip(skipCount)
                           .Take(limit)
                           .Include(p => p.Author)
                           .Include(p => p.Comments)
                           .Include(p => p.MediaItems)
                           .Include(p => p.Votes)
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
                posts
            });
        }

    }
}
