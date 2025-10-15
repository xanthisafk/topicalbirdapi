using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Data.Constants;
using TopicalBirdAPI.Data.DTO.AuthDTO;
using TopicalBirdAPI.Data.DTO.UsersDTO;
using TopicalBirdAPI.Helpers;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<Users> _signInManager;
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext ctx, UserManager<Users> userManager, ILogger<AuthController> logger, SignInManager<Users> signInManager)
        {
            _userManager = userManager;
            _context = ctx;
            _logger = logger;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterToTopicalbird([FromForm] CreateUserRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((await _context.Users.FirstOrDefaultAsync(u => u.Handle == dto.Handle) != null))
            {
                return Conflict(new { message = ErrorMessages.UserHandleConflict });
            }

            if ((await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email) != null))
            {
                return Conflict(new { message = ErrorMessages.UserEmailConflict });
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
                if (dto.Icon != null)
                {
                    var userFolder = Path.Combine("wwwroot/content/uploads/users", user.Id.ToString().ToLower().Replace("-", "_"));
                    var temp = await FileUploadHelper.SaveFile(dto.Icon, userFolder, "icon");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        iconPath = temp;
                    }
                }
                user.Icon = iconPath;
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create user. Errors: {@Errors}", result.Errors);
                    return BadRequest(new { message = ErrorMessages.UserCantCreate, errors = result.Errors });
                }

                return Ok(new { message = SuccessMessages.UserCreated, user = UserResponse.FromUser(user, false) });
            }
            catch (InvalidDataException idx)
            {
                _logger.LogWarning("Failed to create user. User error. Errors: {@Errors}", idx.Message);
                return BadRequest(new { message = idx.Message });
            }
            catch (Exception ex)
            {
                if (iconPath != "/content/assets/defaults/pp_256.png")
                {
                    FileUploadHelper.DeleteFile(iconPath);
                }
                _logger.LogCritical("Failed to create user. Errors: {@Errors}", ex.Message);
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LogInToTopicalbird([FromForm] UserLoginRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: dto.RememberMe);
                    return Ok(new { message = SuccessMessages.UserSignIn });
                }
            }

            return Unauthorized(new { message = ErrorMessages.InvalidCredentials });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> LogOutFromTopicalbird()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = SuccessMessages.UserSignOut });
        }

        [HttpPatch("password")]
        [Authorize]
        public async Task<IActionResult> LogOutFromTopicalbird([FromForm] ChangePasswordRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
                    return BadRequest(new { message = ErrorMessages.InvalidPassword });
                }

                _logger.LogError("Failed to change password for user {UserId}. Errors: {@Errors}", currentUser.Id, result.Errors);
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
            await _signInManager.SignInAsync(currentUser, isPersistent: true);
            return Ok(new { message = SuccessMessages.PasswordChanged });
        }

    }
}
