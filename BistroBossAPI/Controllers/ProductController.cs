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
        public async Task<IActionResult> Add(ProduktAddDto dto, string? nowaKategoria, IFormFile? zdjeciePlik)
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

                string ? errorMessage;
                try
                {
                    var errorObject = JsonDocument.Parse(responseContent).RootElement;
                    errorMessage = errorObject.GetProperty("message").GetString();
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Wysyłanie żądania do api
            var response = await _httpClient.GetAsync($"http://localhost:7000/api/products/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie odnaleziono podanego produktu!";
                return RedirectToAction("Index", "Menu");
            }
            else
            {

                var json = await response.Content.ReadAsStringAsync();
                var produkt = JsonSerializer.Deserialize<ProduktEditDto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                await UstawListeKategorii();
                return View(produkt);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProduktEditDto dto, string? nowaKategoria, IFormFile? zdjeciePlik)
        {
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

            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"http://localhost:7000/api/products/{id}?nowaKategoria={nowaKategoria}",content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Produkt został pomyślnie edytowany!";
                return RedirectToAction("Index", "Menu");
            }

            var errorJson = await response.Content.ReadAsStringAsync();
            var error = JsonDocument.Parse(errorJson).RootElement.GetProperty("message").GetString();

            TempData["ErrorMessage"] = error;

            await UstawListeKategorii();
            return View(dto);
        }
    }
}