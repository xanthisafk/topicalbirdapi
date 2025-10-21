using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Data.API;
using TopicalBirdAPI.Data.Constants;
using TopicalBirdAPI.Data.DTO.AuthDTO;
using TopicalBirdAPI.Data.DTO.UsersDTO;
using TopicalBirdAPI.Helpers;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Controllers
{
    /// <summary>
    /// controls authentication
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        #region Constructor
        private readonly SignInManager<Users> _signInManager;
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;
        private readonly LoggingHelper _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public AuthController(AppDbContext ctx, UserManager<Users> userManager, LoggingHelper logger, SignInManager<Users> signInManager)
        {
            _userManager = userManager;
            _context = ctx;
            _logger = logger;
            _signInManager = signInManager;
        }
        #endregion

        #region CREATE Operations

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <response code="200">Returns the newly created user.</response>
        /// <response code="400">If the data provided is not valid.</response>
        /// <response code="403">If you are logged in.</response>
        /// <response code="409">If username or email is already taken.</response>
        /// <response code="500">If an unexpected error happens.</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<UserResponse>))]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterToTopicalbird([FromForm] CreateUserRequest dto)
        {
            // Invalid Data
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.CreateFromModelState(ModelState));
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser != null && !currentUser.IsAdmin)
            {
                return StatusCode(403, ErrorResponse.Create(ErrorMessages.ForbiddenAction));
            }

            // Username already exists
            if ((await _context.Users.FirstOrDefaultAsync(u => u.Handle == dto.Handle) != null))
            {
                return Conflict(ConflictResponse.Create<string>(dto.Handle, ErrorMessages.UserHandleConflict));
            }

            // Email already in use
            if ((await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email) != null))
            {
                return Conflict(ConflictResponse.Create<string>(dto.Email, ErrorMessages.UserEmailConflict));
            }


            string iconPath = "/content/assets/defaults/pp_256.png";
            try
            {
                var user = new Users
                {
                    Id = Guid.NewGuid(),
                    Handle = dto.Handle.ToLower(),
                    UserName = dto.Handle.ToLower(),
                    Email = dto.Email,
                    DisplayName = dto.DisplayName ?? dto.Handle.ToLower(),
                    CreatedAt = DateTime.UtcNow,
                };

                // Upload image if file exists
                if (dto.Icon != null)
                {
                    var userFolder = Path.Combine("wwwroot/content/uploads/users", user.Id.ToString().ToLower().Replace("-", "_"));
                    var temp = await FileUploadHelper.SaveFile(dto.Icon, userFolder, user.Handle);
                    if (!string.IsNullOrEmpty(temp))
                    {
                        iconPath = temp;
                    }
                }


                user.Icon = iconPath;
                var result = await _userManager.CreateAsync(user, dto.Password);

                // Could not save user
                if (!result.Succeeded)
                {
                    var code = await _logger.IdError(ErrorMessages.UserCantCreate, result.Errors);
                    return BadRequest(ErrorResponse.Create(ErrorMessages.UserCantCreate, null, code));
                }

                // User saved and logged
                _logger.Info($"Created User. User: {user.Id}");
                var response = SuccessResponse<UserResponse>.Create(SuccessMessages.UserCreated, UserResponse.FromUser(user, false));
                return Ok(response);
            }

            // Image file was incorrect
            catch (InvalidDataException idx)
            {
                string refCode = await _logger.Error("Failed to create user.", idx);
                return BadRequest(ErrorResponse.Create(ErrorMessages.InvalidRequest, idx.Message, refCode));
            }

            // Unexpected
            catch (Exception ex)
            {
                if (iconPath != "/content/assets/defaults/pp_256.png")
                {
                    FileUploadHelper.DeleteFile(iconPath);
                }
                string refCode = await _logger.Crit("Failed to create user.", ex);
                return StatusCode(500, ErrorResponse.Create(ErrorMessages.InternalServerError, null, refCode));
            }
        }
        #endregion

        #region READ Operation
        /// <summary>
        /// Login existing user
        /// </summary>
        /// <response code="200">Logs the user in and sets the cookies.</response>
        /// <response code="400">If the data provided is invalid.</response>
        /// <response code="401">If email or password is incorrect.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
        public async Task<IActionResult> LogInToTopicalbird([FromBody] UserLoginRequest dto)
        {
            // Invalid data
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.CreateFromModelState(ModelState));
            }

            // Check if user exists
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null)
            {
                // Check password
                var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Sign the user in
                    await _signInManager.SignInAsync(user, isPersistent: dto.RememberMe);
                    _logger.Info($"User logged in: {user.Id}");
                    return Ok(SuccessResponse<string>.Create(SuccessMessages.UserSignIn, null));
                }
            }

            _logger.Warn($"User login attempt failed.");
            return Unauthorized(ErrorResponse.Create(ErrorMessages.InvalidCredentials, null, null));
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        /// <response code="200">Confirms user has logged out.</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(SuccessResponse<string>))]
        public async Task<IActionResult> LogOutFromTopicalbird()
        {
            await _signInManager.SignOutAsync();
            return Ok(SuccessResponse<string>.Create(SuccessMessages.UserSignOut, null));
        }
        #endregion

        #region UPDATE Operation

        /// <summary>
        /// Change password
        /// </summary>
        /// <response code="200">Returns confirmation of password change</response>
        /// <response code="400">If the data provided is invalid.</response>
        /// <response code="401">If the user is not logged in.</response>
        [HttpPatch("password")]
        [Authorize]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.CreateFromModelState(ModelState));
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(User, _userManager);
            if (currentUser == null)
            {
                return Unauthorized(new { message = ErrorMessages.UnauthorizedAction });
            }

            var result = await _userManager.ChangePasswordAsync(currentUser, dto.OldPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                string[] items = { "PasswordRequiresNonAlphanumeric", "PasswordRequiresDigit", "PasswordRequiresUpper" };
                if (result.Errors.Any(e => items.Contains(e.Code)))
                {
                    return BadRequest(new { message = ErrorMessages.UserMalformedPassword });
                }

                if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
                {
                    return BadRequest(ErrorResponse.Create(ErrorMessages.InvalidCredentials));
                }
                string refCode = await _logger.IdError($"Failed to change password for user {currentUser.Id}", result.Errors);
                return StatusCode(500, ErrorResponse.Create(ErrorMessages.InternalServerError, null, refCode));
            }
            await _signInManager.SignInAsync(currentUser, isPersistent: true);
            return Ok(SuccessResponse<string>.Create(SuccessMessages.PasswordChanged, null));
        }
        #endregion
    }
}
