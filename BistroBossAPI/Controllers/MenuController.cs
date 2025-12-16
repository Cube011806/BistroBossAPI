using BistroBossAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BistroBossAPI.Controllers
{
    public class MenuController : Controller
    {
        private readonly HttpClient _httpClient;

        public MenuController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("http://localhost:7000/api/products/menu");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie udało się pobrać menu!";
                return View(new List<KategoriaMenuDto>());
            }

            var json = await response.Content.ReadAsStringAsync();
            // Ignorujemy wielkosc liter (bo w modelu jest Id, a w jsonie id i był błąd
            var kategorie = JsonSerializer.Deserialize<List<KategoriaMenuDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(kategorie);
        }
    }
}
