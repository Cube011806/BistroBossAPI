using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BistroBossAPI.Controllers.ApiControllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductControllerAPI : ControllerBase
    {
        private readonly ProductService _productService;

        // Wstrzykujemy Serwis
        public ProductControllerAPI(ProductService productService)
        {
            _productService = productService;
        }

        // Przykład: POST /api/products?nowaKategoria=Napój
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProduktCreateDto dto, [FromQuery] string? nowaKategoria)
        {
            if (!ModelState.IsValid)
            {
                // Ręczne zwrócenie problemu, jeśli dane są strukturalnie niepoprawne
                return BadRequest(ModelState);
            }

            // Kontroler API jest serwerem, a Serwis to jego silnik.
            var (success, produkt, errorMessage) = await _productService.AddProductAsync(dto, nowaKategoria);

            // Obsługa Wyniku
            if (success)
            {
                // 201 Created (Zasób został pomyślnie utworzony).
                return Created($"api/products/{produkt!.Id}", produkt);
            }
            else
            {
                // 400 Bad Request (Dane są niepoprawne).
                return BadRequest(new { message = errorMessage });
            }
        }

        // Przykład: GET /api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var produkt = await _productService.GetProductByIdAsync(id);

            if (produkt == null)
            {
                return NotFound(); // Zwraca 404
            }

            return Ok(produkt); // Zwraca 200 z obiektem Produkt
        }
    }
}