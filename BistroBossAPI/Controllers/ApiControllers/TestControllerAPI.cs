using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BistroBossAPI.Controllers.ApiControllers
{
    [Route("api/authtest")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TestControllerAPI : ControllerBase
    {
        [HttpGet]
        public IActionResult Test()
        {
            return Ok(new string[] { "Ok" });
        }
    }
}
