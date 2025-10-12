using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using TopicalBirdAPI.Constants;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.DTO;
using TopicalBirdAPI.Helpers;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext ctx, UserManager<Users> userManager, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _context = ctx;
            _logger = logger;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return NotFound(new { message = ErrorMessages.ForbiddenAction });
            }
            return Ok(new { user = UserResponse.FromUser(currentUser, true) });
        }

        // GET api/users/{id}
        [HttpGet("get/id/{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var targetUser = await _userManager.FindByIdAsync(id.ToString());
            if (targetUser == null)
            {
                return NotFound(new { message = ErrorMessages.UserNotFound });
            }

            bool adminPrivilage = false;
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser != null && currentUser.IsAdmin)
            {
                adminPrivilage = true;
            }

            UserResponse res = UserResponse.FromUser(targetUser, adminPrivilage);

            return Ok(new { user = res });
        }


        // GET api/users/email/{email}
        [HttpGet("get/email/{email}")]
        [Authorize]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                return Forbid(ErrorMessages.ForbiddenAction);
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { message = ErrorMessages.UserNotFound });
            }

            return Ok(new { user = UserResponse.FromUser(user, true) });
        }


        // PUT api/users/{id}
        [HttpPut("update/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest dto)
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorMessages.UnauthorizedAction);
            }

            var targetUser = await _userManager.FindByIdAsync(id.ToString());
            if (targetUser == null)
            {
                return NotFound(new { message = ErrorMessages.UserNotFound });
            }

            bool allowUpdate = (currentUser.Id == targetUser.Id) || currentUser.IsAdmin;

            if (!allowUpdate)
            {
                return Forbid(ErrorMessages.ForbiddenAction);
            }

            targetUser.DisplayName = dto.DisplayName ?? targetUser.DisplayName;
            targetUser.Icon = dto.Icon ?? targetUser.Icon;

            var result = await _userManager.UpdateAsync(targetUser);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user {UserId}. Errors: {@Errors}", targetUser.Id, result.Errors);
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }

            return Ok(new 
            { 
                message = SuccessMessages.UserUpdated,
                user = UserResponse.FromUser(targetUser, currentUser.IsAdmin)
            });
        }

        [HttpPut("ban/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> BanUser(Guid id)
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorMessages.UnauthorizedAction);
            }

            if (!currentUser.IsAdmin || currentUser.Id == id)
            {
                return Forbid(ErrorMessages.ForbiddenAction);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(new { message = ErrorMessages.UserNotFound});
            }

            if (user.IsBanned)
            {
                return BadRequest(new { message = ErrorMessages.UserBanned });
            }

            user.IsBanned = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user {UserId}. Errors: {@Errors}", user.Id, result.Errors);
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }

            return Ok(new { message = SuccessMessages.UserBanned });
        }

        [HttpPut("unban/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> UnbanUser(Guid id)
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorMessages.UnauthorizedAction);
            }

            if (!currentUser.IsAdmin || currentUser.Id == id)
            {
                return Forbid(ErrorMessages.ForbiddenAction);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(new { message = ErrorMessages.UserNotFound });
            }

            if (!user.IsBanned)
            {
                return BadRequest(new { message = ErrorMessages.UserAlreadyUnbanned });
            }

            user.IsBanned = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user {UserId}. Errors: {@Errors}", user.Id, result.Errors);
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }

            return Ok(new { message = SuccessMessages.UserUnbanned });
        }

        [HttpPut("promote/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> PromoteToAdmin(Guid id)
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorMessages.UnauthorizedAction);
            }

            if (!currentUser.IsAdmin || currentUser.Id == id)
            {
                return Forbid(ErrorMessages.ForbiddenAction);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(new { message = ErrorMessages.UserNotFound });
            }

            if (user.IsAdmin)
            {
                return BadRequest(new { message = ErrorMessages.UserAlreadyAdmin });
            }

            user.IsAdmin = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user {UserId}. Errors: {@Errors}", user.Id, result.Errors);
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }

            return Ok(new { message = SuccessMessages.UserUpdated });

        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                return Forbid(ErrorMessages.ForbiddenAction);
            }

            var users = await _userManager.Users
                .Select(u => UserResponse.FromUser(u, true))
                .ToListAsync();

            return Ok(new {users});
        }

    }
}
