using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BistroBossAPI.Controllers.ApiControllers
{
    [Route("api/manage")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ManageControllerAPI : ControllerBase
    {
        private readonly ManageService _service;
        private readonly UserManager<Uzytkownik> _userManager;

        public ManageControllerAPI(ManageService service, UserManager<Uzytkownik> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int? search)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            return Ok(await _service.GetAllOrdersAsync(search));
        }

        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var result = await _service.GetOrderAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPut("orders/{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var result = await _service.SetStatusAsync(id, 0);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        [HttpPut("orders/{id}/prepare")]
        public async Task<IActionResult> Prepare(int id)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var result = await _service.SetStatusAsync(id, 2);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        [HttpPut("orders/{id}/delivery")]
        public async Task<IActionResult> Delivery(int id)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var result = await _service.SetInDeliveryAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        [HttpPut("orders/{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var result = await _service.SetStatusAsync(id, 4);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("orders/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var result = await _service.DeleteOrderAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            return Ok(await _service.GetUsersAsync());
        }

        [HttpPut("users/{id}/admin")]
        public async Task<IActionResult> MakeAdmin(string id)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var result = await _service.SetAdminAsync(id, true);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        [HttpPut("users/{id}/unadmin")]
        public async Task<IActionResult> UnmakeAdmin(string id)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var result = await _service.SetAdminAsync(id, false);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> RemoveUser(string id)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var result = await _service.RemoveUserAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }
    }
}
