// Controllers/ProductController.cs
using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BistroBossAPI.Controllers
{
    public class ProductController : BaseController
    {
        private readonly UserManager<Uzytkownik> _userManager;
        private readonly ProductService _productService;
        private readonly HttpClient _httpClient; // Klient HTTP

        public ProductController(
            ProductService productService,
            UserManager<Uzytkownik> userManager,
            HttpClient httpClient) // Wstrzyknięcie klienta HTTP
            : base(productService)
        {
            _userManager = userManager;
            _productService = productService;
            _httpClient = httpClient;
        }

        private async Task UstawListeKategorii()
        {
            var kategorie = await _productService.GetKategorieAsync();

            var listaKategorii = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Wybierz kategorię produktu --", Selected = true }
            };
            listaKategorii.AddRange(kategorie.Select(k => new SelectListItem
            {
                Value = k.Id.ToString(),
                Text = k.Nazwa
            }));
            ViewBag.ListaKategorii = listaKategorii;
        }

        public IActionResult Index() 
        { 
            return View(); 
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            await UstawListeKategorii();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(ProduktCreateDto dto, string? nowaKategoria, IFormFile? zdjeciePlik)
        {
            // Zapis do pliku
            if (zdjeciePlik != null && zdjeciePlik.Length > 0)
            {
                var folderPath = Path.Combine("wwwroot", "images", "produkty");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(zdjeciePlik.FileName);
                var filePath = Path.Combine(folderPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await zdjeciePlik.CopyToAsync(stream);
                }
                dto.Zdjecie = "/images/produkty/" + uniqueFileName;
            }

            // Wysyłanie żądania do api
            var json = JsonSerializer.Serialize(dto);
            var apiPath = $"http://localhost:7000/api/products?nowaKategoria={nowaKategoria}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiPath, content);

            // Obsługa wyniku z api
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Produkt został pomyślnie dodany!";
                return RedirectToAction("Index", "Menu");
            }
            else
            {
                // Próba odczytu komunikatu błędu z ciała odpowiedzi API
                var responseContent = await response.Content.ReadAsStringAsync();

                string errorMessage;
                try
                {
                    var errorObject = JsonDocument.Parse(responseContent).RootElement;
                    errorMessage = errorObject.GetProperty("message").GetString() ?? $"Błąd API: {response.ReasonPhrase}";
                }
                catch
                {
                    errorMessage = $"Nieznany błąd po stronie API: Kod {response.StatusCode}.";
                }

                TempData["ErrorMessage"] = errorMessage;
                await UstawListeKategorii();
                return View(dto);
            }
        }
    }
}