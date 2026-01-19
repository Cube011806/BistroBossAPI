using BistroBossAPI.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BistroBossAPI
{
    public static class UserManagerExtensions
    {
        public static bool IsNotAllowedInEndpoint(this UserManager<Uzytkownik> userManager, string userId, ClaimsPrincipal claims)
        {
            return claims.Identity.IsAuthenticated && userManager.GetUserId(claims) != userId;
        }

        public static async Task<bool> IsAdminAsync(this UserManager<Uzytkownik> userManager, ClaimsPrincipal claims)
        {
            var user = await userManager.GetUserAsync(claims);
            return user?.AccessLevel == 2;
        }
    }
}
