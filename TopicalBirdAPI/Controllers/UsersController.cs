using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Data.API;
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
        private readonly LoggingHelper _logger;

        public UsersController(AppDbContext ctx, UserManager<Users> userManager, LoggingHelper logger)
        {
            _userManager = userManager;
            _context = ctx;
            _logger = logger;
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <remarks>
        /// Admin only endpoint.
        /// </remarks>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessReponse<List<UserResponse>>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
        [Produces("application/json")]
        public async Task<IActionResult> GetAllUsers()
        {
            // Check for admin privilege
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                return StatusCode(403, ErrorResponse.Create(ErrorMessages.ForbiddenAction));
            }

            // Fetch and map all users
            var users = await _userManager.Users
                .AsQueryable()
                .Select(u => UserResponse.FromUser(u, true))
                .ToListAsync();

            return Ok(SuccessReponse<List<UserResponse>>.Create(SuccessMessages.OperationCompleted, users));
        }

        /// <summary>
        /// Gets the currently authenticated user's details.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessReponse<UserResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
        [Produces("application/json")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Get current user
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorResponse.Create(ErrorMessages.UnauthorizedAction));
            }
            // Return user data
            return Ok(SuccessReponse<UserResponse>.Create(SuccessMessages.OperationCompleted, UserResponse.FromUser(currentUser, true)));
        }

        /// <summary>
        /// Gets a user's details by their unique ID.
        /// </summary>
        /// <param name="id">Id of user</param>
        [HttpGet("get/id/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessReponse<UserResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [Produces("application/json")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            // Find target user
            var targetUser = await _userManager.FindByIdAsync(id.ToString());
            if (targetUser == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.UserNotFound));
            }

            // Determine if admin privileges apply
            bool adminPrivilage = false;
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser != null && currentUser.IsAdmin)
            {
                adminPrivilage = true;
            }

            // Map and return user data
            UserResponse res = UserResponse.FromUser(targetUser, adminPrivilage);

            return Ok(SuccessReponse<UserResponse>.Create(SuccessMessages.OperationCompleted, res));
        }

        /// <summary>
        /// Gets a user's details by their email address.
        /// </summary>
        /// <param name="email">Email address of user</param>
        /// <remarks>
        /// Admin only endpoint.
        /// </remarks>
        [HttpGet("get/email/{email}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessReponse<UserResponse>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [Produces("application/json")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            // Check for admin privilege
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                return StatusCode(403, ErrorResponse.Create(ErrorMessages.ForbiddenAction));
            }

            // Find user by email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.UserNotFound));
            }

            // Return user data with admin privilege set to true
            return Ok(SuccessReponse<UserResponse>.Create(SuccessMessages.OperationCompleted, UserResponse.FromUser(user, true)));
        }

        /// <summary>
        /// Gets a user's details by their unique username/handle.
        /// </summary>
        /// <param name="username">Username/Handle of user</param>
        [HttpGet("get/username/{username}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessReponse<UserResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [Produces("application/json")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            // Determine if current user has admin privilege
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            bool admin = currentUser != null ? currentUser.IsAdmin : false;

            // Find user by handle (username)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Handle == username);
            if (user == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.UserNotFound));
            }

            // Return user data
            return Ok(SuccessReponse<UserResponse>.Create(SuccessMessages.OperationCompleted, UserResponse.FromUser(user, admin)));
        }

        /// <summary>
        /// Searches for users by matching query with Handle or DisplayName.
        /// </summary>
        /// <param name="query">Text to search</param>
        /// <remarks>
        /// Requires authentication.
        /// </remarks>
        [HttpGet("search")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessReponse<List<UserResponse>>))]
        [Produces("application/json")]
        public async Task<IActionResult> GetBySearch(string query)
        {
            // Determine if current user has admin privilege
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            bool admin = currentUser != null ? currentUser.IsAdmin : false;

            // Search for users
            var users = await _context.Users
                .Where(u => u.Handle.Contains(query) || u.DisplayName.Contains(query))
                .Select(u => UserResponse.FromUser(u, admin))
                .ToListAsync();

            // Return search results
            return Ok(SuccessReponse<List<UserResponse>>.Create(SuccessMessages.OperationCompleted, users));
        }

        /// <summary>
        /// Updates an authenticated user's details (DisplayName and/or Icon).
        /// </summary>
        /// <param name="id">Id of user</param>
        /// <param name="dto">Data Transfer Object for UserUpdate</param>
        /// <remarks>
        /// Sample request:
        /// 
        ///      PATCH api/Users/update/3fa85f64-5717-4562-b3fc-2c963f66afa6
        ///      Accepts: multipart/form-data
        ///      
        ///      {
        ///          DisplayName = string,
        ///          Icon = File
        ///      }
        ///      
        /// </remarks>
        [HttpPatch("update/{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessReponse<UserResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromForm] UpdateUserRequest dto)
        {
            // Invalid data check
            if (dto.DisplayName == null && dto.Icon == null)
            {
                return BadRequest(ErrorResponse.Create(ErrorMessages.InvalidRequest, "Displayname and Icon can not be empty", null));
            }

            // Not logged in check
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorResponse.Create(ErrorMessages.UnauthorizedAction));
            }

            // User not found check
            var targetUser = await _userManager.FindByIdAsync(id.ToString());
            if (targetUser == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.UserNotFound));
            }

            // Authorization check (self or admin)
            bool allowUpdate = (currentUser.Id == targetUser.Id) || currentUser.IsAdmin;
            if (!allowUpdate)
            {
                return StatusCode(403, ErrorResponse.Create(ErrorMessages.ForbiddenAction));
            }

            // Update display name
            targetUser.DisplayName = dto.DisplayName ?? targetUser.DisplayName;

            string iconPath = targetUser.Icon ?? "/content/assets/defaults/pp_256.png";
            string originalIcon = targetUser.Icon ?? "/content/assets/defaults/pp_256.png";
            try
            {
                // Handle new icon upload
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

                // Persist changes
                var result = await _userManager.UpdateAsync(targetUser);
                if (!result.Succeeded)
                {
                    // Log Identity errors and return 500
                    string refKey = await _logger.IdError("Failed to update user.", result.Errors);
                    return StatusCode(500, ErrorResponse.Create(ErrorMessages.InternalServerError, null, refKey));
                }

                // Success response
                return Ok(
                    SuccessReponse<UserResponse>.Create(
                        SuccessMessages.UserUpdated,
                        UserResponse.FromUser(
                            targetUser, currentUser.IsAdmin)));
            }
            catch (InvalidDataException idx)
            {
                // Log and return 400 for file upload issues
                string refKey = await _logger.Error("Failed to upload images.", idx);
                return BadRequest(ErrorResponse.Create(ErrorMessages.FailedToProcessMedia, idx.Message, refKey));
            }
            catch (Exception ex)
            {
                // Clean up partially uploaded file on general error
                if (iconPath != originalIcon && iconPath != "/content/assets/defaults/pp_256.png")
                {
                    FileUploadHelper.DeleteFile(iconPath);
                }
                // Log critical error and return 500
                string refKey = await _logger.Crit("Failed to update user.", ex);
                return StatusCode(500, ErrorResponse.Create(ErrorMessages.InternalServerError, null, refKey));
            }
        }

        /// <summary>
        /// Bans a user by ID.
        /// </summary>
        /// <param name="id">Id of user getting banned</param>
        /// <remarks>
        /// Admin only endpoint.
        /// </remarks>
        [HttpPatch("ban/{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessReponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        [Produces("application/json")]
        public async Task<IActionResult> BanUser(Guid id)
        {
            // Check logged in
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorResponse.Create(ErrorMessages.UnauthorizedAction));
            }

            // Check if admin and not banning self
            if (!currentUser.IsAdmin || currentUser.Id == id)
            {
                return StatusCode(403, ErrorResponse.Create(ErrorMessages.ForbiddenAction));
            }

            // Check user exists
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.UserNotFound));
            }

            // Check user already banned
            if (user.IsBanned)
            {
                return BadRequest(ErrorResponse.Create(ErrorMessages.UserBanned));
            }

            // Perform ban and update
            user.IsBanned = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // Log errors and return 500
                string refCode = await _logger.IdError($"Failed to ban user: {user.Id}.", result.Errors);
                return StatusCode(500, ErrorResponse.Create(ErrorMessages.InternalServerError, null, refCode));
            }
            // The existing code returned an ErrorResponse for success, changed to SuccessResponse
            return Ok(SuccessReponse<string>.Create(SuccessMessages.UserBanned, null));
        }

        /// <summary>
        /// Unbans a user by ID.
        /// </summary>
        /// <param name="id">Id of user getting unbanned</param>
        /// <remarks>
        /// Admin only endpoint.
        /// </remarks>
        [HttpPatch("unban/{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessReponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [Produces("application/json")]
        public async Task<IActionResult> UnbanUser(Guid id)
        {
            // Check logged in
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorResponse.Create(ErrorMessages.UnauthorizedAction));
            }

            // Check if admin and not unbanning self
            if (!currentUser.IsAdmin || currentUser.Id == id)
            {
                return StatusCode(403, ErrorResponse.Create(ErrorMessages.ForbiddenAction));
            }

            // Check user exists
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.UserNotFound));
            }

            // Check user is not banned
            if (!user.IsBanned)
            {
                return BadRequest(ErrorResponse.Create(ErrorMessages.UserAlreadyUnbanned));
            }

            // Perform unban and update
            user.IsBanned = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // Log errors and return 500
                string refCode = await _logger.IdError($"Failed to unban user: {user.Id}.", result.Errors);
                return StatusCode(500, ErrorResponse.Create(ErrorMessages.InternalServerError, null, refCode));
            }

            // Success response
            return Ok(SuccessReponse<string>.Create(SuccessMessages.UserUnbanned, null));
        }

        /// <summary>
        /// Promotes a user to an admin.
        /// </summary>
        /// <param name="id">Id of user getting promoted</param>
        /// <remarks>
        /// Admin only endpoint.
        /// </remarks>
        [HttpPatch("promote/{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessReponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        [Produces("application/json")]
        public async Task<IActionResult> PromoteToAdmin(Guid id)
        {
            // Check logged in
            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(ErrorResponse.Create(ErrorMessages.UnauthorizedAction));
            }

            // Check if admin and not promoting self
            if (!currentUser.IsAdmin || currentUser.Id == id)
            {
                return StatusCode(403, ErrorResponse.Create(ErrorMessages.ForbiddenAction));
            }

            // Check user exists
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(ErrorResponse.Create(ErrorMessages.UserNotFound));
            }

            // Check user already admin
            if (user.IsAdmin)
            {
                return BadRequest(ErrorResponse.Create(ErrorMessages.UserAlreadyAdmin));
            }

            // Promote user and update
            user.IsAdmin = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // Log errors and return 500
                string refCode = await _logger.IdError($"Failed to promote user: {user.Id}.", result.Errors);
                return StatusCode(500, ErrorResponse.Create(ErrorMessages.InternalServerError, null, refCode));
            }

            // Success response
            return Ok(SuccessReponse<string>.Create(SuccessMessages.UserUpdated, null));
        }
    }
}