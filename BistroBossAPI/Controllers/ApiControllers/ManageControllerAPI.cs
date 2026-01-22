using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BistroBossAPI.Controllers.ApiControllers
{
    [Route("api/manage")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ManageControllerAPI : ControllerBase
    {
        private readonly ManageService _service;

        public ManageControllerAPI(ManageService service)
        {
            _service = service;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int? search)
        {
            return Ok(await _service.GetAllOrdersAsync(search));
        }

        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var result = await _service.GetOrderAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPut("orders/{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            return Ok(await _service.SetStatusAsync(id, 0));
        }

        [HttpPut("orders/{id}/prepare")]
        public async Task<IActionResult> Prepare(int id)
        {
            return Ok(await _service.SetStatusAsync(id, 2));
        }

        [HttpPut("orders/{id}/delivery")]
        public async Task<IActionResult> Delivery(int id)
        {
            return Ok(await _service.SetInDeliveryAsync(id));
        }

        [HttpPut("orders/{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            return Ok(await _service.SetStatusAsync(id, 4));
        }

        [HttpDelete("orders/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            return Ok(await _service.DeleteOrderAsync(id));
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _service.GetUsersAsync());
        }

        [HttpPut("users/{id}/admin")]
        public async Task<IActionResult> MakeAdmin(string id)
        {
            return Ok(await _service.SetAdminAsync(id, true));
        }

        [HttpPut("users/{id}/unadmin")]
        public async Task<IActionResult> UnmakeAdmin(string id)
        {
            return Ok(await _service.SetAdminAsync(id, false));
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> RemoveUser(string id)
        {
            return Ok(await _service.RemoveUserAsync(id));
        }
    }
}
