using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BistroBossAPI.Controllers.ApiControllers
{
    [Route("api/baskets")]
    [ApiController]
    [AllowAnonymous]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BasketControllerAPI : ControllerBase
    {
        private readonly BasketService _basketService;
        private readonly UserManager<Uzytkownik> _userManager;

        // Wstrzykujemy Serwis
        public BasketControllerAPI(BasketService basketService, UserManager<Uzytkownik> userManager)
        {
            _basketService = basketService;
            _userManager = userManager;
        }

        //Przykład: GET /api/baskets/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetBasket(string userId)
        {
            if (userId != "GUEST" && _userManager.IsNotAllowedInEndpoint(userId, User))
            {
                return Unauthorized();
            }

            var basket = await _basketService.GetBasketForUserAsync(userId);
            return Ok(basket);
        }

        //Przykład: POST /api/baskets?userId=123&produktId=123
        [HttpPost]
        public async Task<IActionResult> AddToBasket([FromQuery] string userId, [FromQuery] int produktId)
        {
            if (userId != "GUEST" && _userManager.IsNotAllowedInEndpoint(userId, User))
            {
                return Unauthorized();
            }

            var result = await _basketService.AddToBasketAsync(userId, produktId);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Dodano produkt do koszyka!" });
        }

        //Przykład: DELETE /api/baskets/{userId}/products/{koszykProduktId}
        [HttpDelete("{userId}/products/{koszykProduktId}")]
        public async Task<IActionResult> RemoveFromBasket(string userId, int koszykProduktId)
        {
            if (userId != "GUEST" && _userManager.IsNotAllowedInEndpoint(userId, User))
            {
                return Unauthorized();
            }

            var result = await _basketService.RemoveFromBasketAsync(userId, koszykProduktId);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = result.Message });
        }

        //Przykład: POST /api/baskets/guest
        [HttpPost("guest")]
        public async Task<IActionResult> SetGuestBasket([FromBody] KoszykGuestDto dto)
        {
            var success = await _basketService.SetGuestBasketAsync(dto);

            if (!success)
                return BadRequest(new { message = "Nie udało się zapisać koszyka gościa." });

            return Ok();
        }
    }
}
