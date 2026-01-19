using BistroBossAPI.Models;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BistroBossAPI.Controllers.ApiControllers
{
    [Route("api/checkout")]
    [ApiController]
    [AllowAnonymous]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CheckoutControllerAPI : ControllerBase
    {
        private readonly CheckoutService _checkoutService;
        private readonly UserManager<Uzytkownik> _userManager;

        public CheckoutControllerAPI(CheckoutService checkoutService, UserManager<Uzytkownik> userManager)
        {
            _checkoutService = checkoutService;
            _userManager = userManager;
        }

        //Przykład: GET /api/checkout/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCheckoutData(string userId)
        {
            if (userId != "GUEST" && _userManager.IsNotAllowedInEndpoint(userId, User))
            {
                return Unauthorized();
            }

            var result = await _checkoutService.GetCheckoutDataAsync(userId);
            return Ok(result);
        }
    }
}
