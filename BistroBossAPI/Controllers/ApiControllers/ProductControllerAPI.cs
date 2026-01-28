using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Humanizer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BistroBossAPI.Controllers.ApiControllers
{
    [Route("api/products")]
    [ApiController]
    [AllowAnonymous]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductControllerAPI : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly UserManager<Uzytkownik> _userManager;

        public ProductControllerAPI(ProductService productService, UserManager<Uzytkownik> userManager)
        {
            _productService = productService;
            _userManager = userManager;
        }

        // Przykład: GET /api/products/menu
        [HttpGet("menu")]
        public async Task<IActionResult> GetMenu()
        {
            var kategorie = await _productService.GetMenuDtoAsync();
            return Ok(kategorie);
        }

        // Przykład: GET /api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var produkt = await _productService.GetProductByIdAsync(id);

            if (produkt == null)
            {
                return NotFound(); 
            }

            return Ok(produkt); 
        }

        // Przykład: POST /api/products?nowaKategoria=Napój
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProduktAddDto dto, [FromQuery] string? nowaKategoria)
        {
            if(!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, produkt, errorMessage) = await _productService.AddProductAsync(dto, nowaKategoria);

            if (success)
            {
                return Created($"api/products/{produkt!.Id}", produkt);
            }
            else
            {
                return BadRequest(new { message = errorMessage });
            }
        }

        //Przykład: PUT /api/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProduktEditDto dto, [FromQuery] string? nowaKategoria)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var (success, produkt, errorMessage) = await _productService.UpdateProductAsync(id, dto, nowaKategoria);

            if (success)
                return Ok(produkt);

            return BadRequest(new { message = errorMessage });
        }
        
        //Przykład: DELETE /api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var (success, errorMessage) = await _productService.DeleteProductAsync(id);

            if (success)
                return Ok(new { message = "Produkt usunięty" });

            return BadRequest(new { message = errorMessage });
        }

    }
}