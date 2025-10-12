using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI.Helpers
{
    public static class UserHelper
    {
        public static async Task<Users?> GetCurrentUserAsync(ClaimsPrincipal user, UserManager<Users> userManager)
        {
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return null;

            return await userManager.FindByIdAsync(userId);
        }
    }

}
