using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BistroBossAPI.Controllers
{
    public class MenuController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<Uzytkownik> _userManager;

        public MenuController(HttpClient httpClient, UserManager<Uzytkownik> userManager)
        {
            _httpClient = httpClient;
            _userManager = userManager;
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

        [HttpPost]
        public async Task<IActionResult> AddToBasket(int produktId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Musisz być zalogowany, aby dodać produkt do koszyka.";
                return RedirectToAction("Index", "Menu");
            }

            var userId = _userManager.GetUserId(User);

            var response = await _httpClient.PostAsync(
                $"http://localhost:7000/api/baskets?userId={userId}&produktId={produktId}",
                null
            );

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Produkt został dodany do koszyka!";
            else
                TempData["ErrorMessage"] = "Nie udało się dodać produktu do koszyka.";

            return RedirectToAction("Index", "Menu");
        }
    }
}
