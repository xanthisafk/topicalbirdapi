using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopicalBirdAPI.Data;
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

        [HttpGet]
        public async Task<IActionResult> GetAllNests()
        {
            var response = await _context.Nests
                .Include(n => n.Moderator)
                .Include(n => n.Posts)
                .Select(n => new
                {
                    n.Id,
                    n.Icon,
                    n.Description,
                    n.DisplayName,
                    n.CreatedAt,
                    Moderator = new
                    {
                        n.Moderator.Id,
                        n.Moderator.DisplayName,
                        n.Moderator.Icon,
                    },
                    n.Posts
                }).ToListAsync();

            return Ok(new { nests = response });
        }

    }
}
