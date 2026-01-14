// Controllers/ProductController.cs
using BistroBossAPI.Controllers.ApiControllers;
using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BistroBossAPI.Controllers
{
    public class ProductController : BaseController
    {
        private readonly UserManager<Uzytkownik> _userManager;
        private readonly HttpClient _httpClient; // Klient HTTP
        private readonly ProductService _productService;
        private readonly IConfiguration _configuration;
        

        public ProductController(
            ProductService productService,
            BasketService basketService,
            CheckoutService checkoutService,
            OrderService orderService,
            UserManager<Uzytkownik> userManager,
            HttpClient httpClient,
            IConfiguration configuration) // Wstrzyknięcie klienta HTTP
            : base(productService, basketService, checkoutService, orderService)
        {
            _userManager = userManager;
            _productService = productService;
            _httpClient = httpClient;
            _configuration = configuration;
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

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }

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
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }

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

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }

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

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }

            var response = await _httpClient.GetAsync($"http://localhost:7000/api/products/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie odnaleziono podanego produktu!";
                return RedirectToAction("Index", "Menu");
            }

            var json = await response.Content.ReadAsStringAsync();
            var produkt = JsonSerializer.Deserialize<ProduktDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(produkt);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }

            var response = await _httpClient.DeleteAsync($"http://localhost:7000/api/products/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Produkt został pomyślnie usunięty!";
                return RedirectToAction("Index", "Menu");
            }

            var errorJson = await response.Content.ReadAsStringAsync();
            string error;

            try
            {
                error = JsonDocument.Parse(errorJson).RootElement.GetProperty("message").GetString();
            }
            catch
            {
                error = "Nieznany błąd po stronie API.";
            }

            TempData["ErrorMessage"] = error;
            return RedirectToAction("Index", "Menu");
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }

            var response = await _httpClient.GetAsync($"http://localhost:7000/api/products/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie odnaleziono podanego produktu!";
                return RedirectToAction("Index", "Menu");
            }

            var json = await response.Content.ReadAsStringAsync();
            var produkt = JsonSerializer.Deserialize<ProduktDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var nazwaKategorii = (await _productService.GetKategorieAsync()).FirstOrDefault(k => k.Id == produkt.KategoriaId)?.Nazwa;

            ViewBag.KategoriaNazwa = nazwaKategorii;

            return View(produkt);
        }

    }
}