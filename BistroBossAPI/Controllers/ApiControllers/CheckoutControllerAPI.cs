using BistroBossAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BistroBossAPI.Controllers.ApiControllers
{
    [Route("api/checkout")]
    [ApiController]
    public class CheckoutControllerAPI : ControllerBase
    {
        private readonly CheckoutService _checkoutService;

        public CheckoutControllerAPI(CheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        //Przykład: GET /api/checkout/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCheckoutData(string userId)
        {
            var result = await _checkoutService.GetCheckoutDataAsync(userId);
            return Ok(result);
        }
    }
}
