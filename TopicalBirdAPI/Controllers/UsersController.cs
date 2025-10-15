using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Data.Constants;
using TopicalBirdAPI.Data.DTO.AuthDTO;
using TopicalBirdAPI.Data.DTO.UsersDTO;
using TopicalBirdAPI.Helpers;
using TopicalBirdAPI.Migrations;
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
                return Unauthorized(new { message = ErrorMessages.UnauthorizedAction });
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
                return StatusCode(403, new { message = ErrorMessages.ForbiddenAction });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { message = ErrorMessages.UserNotFound });
            }

            return Ok(new { user = UserResponse.FromUser(user, true) });
        }

        [HttpGet("get/username/{username}")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            bool admin = currentUser != null ? currentUser.IsAdmin : false;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Handle == username);
            if (user == null)
            {
                return NotFound(new { message = ErrorMessages.UserNotFound });
            }

            return Ok(new { user = UserResponse.FromUser(user, admin) });
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> GetBySearch(string query)
        {
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            bool admin = currentUser != null ? currentUser.IsAdmin : false;

            var users = await _context.Users
                .Where(u => u.Handle.Contains(query) || u.DisplayName.Contains(query))
                .Select(u => UserResponse.FromUser(u, admin))
                .ToListAsync();

            return Ok(new { users });
        }

        // PUT api/users/{id}
        [HttpPut("update/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(Guid id, [FromForm] UpdateUserRequest dto)
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
                return StatusCode(403, new { message = ErrorMessages.ForbiddenAction });
            }

            if (!string.IsNullOrWhiteSpace(dto.Handle))
            {
                return BadRequest(new { message = ErrorMessages.UserHandleChange });
            }

            targetUser.DisplayName = dto.DisplayName ?? targetUser.DisplayName;
            targetUser.Handle = dto.Handle;

            string iconPath = "/content/assets/defaults/pp_256.png";
            string originalIcon = targetUser.Icon ?? "";
            try
            {

                if (dto.Icon != null)
                {
                    var userFolder = Path.Combine("wwwroot/content/uploads/users", targetUser.Id.ToString().ToLower().Replace("-", "_"));
                    var temp = await FileUploadHelper.SaveFile(dto.Icon, userFolder, "icon");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        iconPath = temp;
                    }
                }
                targetUser.Icon = iconPath;

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
            catch (InvalidDataException idx)
            {
                _logger.LogInformation("Failed to create nest. Errors: {@Errors}", idx.Message);
                return BadRequest(new { message = idx.Message });
            }
            catch (Exception ex)
            {
                if (iconPath != originalIcon)
                {
                    FileUploadHelper.DeleteFile(iconPath);
                }
                _logger.LogError("Failed to create nest. Errors: {@Errors}", ex.Message);
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }

            
        }


        [HttpPatch("ban/{id:guid}")]
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
                return StatusCode(403, new { message = ErrorMessages.ForbiddenAction });
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

        [HttpPatch("unban/{id:guid}")]
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
                return StatusCode(403, new { message = ErrorMessages.ForbiddenAction });
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

        [HttpPatch("promote/{id:guid}")]
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
                return StatusCode(403, new { message = ErrorMessages.ForbiddenAction });
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
                return StatusCode(403, new { message = ErrorMessages.ForbiddenAction });
            }

            var users = await _userManager.Users
                .AsQueryable()
                .Select(u => UserResponse.FromUser(u, true))
                .ToListAsync();

            return Ok(new {users});
        }

    }
}
