using BistroBossAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace BistroBossAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello World!");
        }

        //[HttpGet]
        //public IActionResult GetAll()
        //{
        //    var products = _context.Produkty.ToList();
        //    return Ok(products);
        //}
    }

}