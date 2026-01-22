using BistroBossAPI.Controllers.ApiControllers;
using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BistroBossAPI.Controllers
{
    public class OrderController : BaseController
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<Uzytkownik> _userManager;
        private readonly IConfiguration _configuration;

        public OrderController(
            HttpClient httpClient,
            UserManager<Uzytkownik> userManager,
            ProductService productService,
            BasketService basketService,
            CheckoutService checkoutService,
            OrderService orderService,
            IConfiguration configuration)
            : base(productService, basketService, checkoutService, orderService)
        {
            _httpClient = httpClient;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> ShowOrder(int id)
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }

            ViewBag.IsGuest = User.Identity.IsAuthenticated ? "0" : "1";

            var response = await _httpClient.GetAsync($"http://localhost:7000/api/orders/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie znaleziono zamówienia.";
                return RedirectToAction("Index", "Home");
            }

            var json = await response.Content.ReadAsStringAsync();
            var order = JsonSerializer.Deserialize<ZamowienieDetailsDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(order);
        }
        public async Task<IActionResult> ShowOrderAdmin(int id)
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }

            ViewBag.IsGuest = User.Identity.IsAuthenticated ? "0" : "1";

            var response = await _httpClient.GetAsync($"http://localhost:7000/api/orders/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie znaleziono zamówienia.";
                return RedirectToAction("Index", "Home");
            }

            var json = await response.Content.ReadAsStringAsync();
            var order = JsonSerializer.Deserialize<ZamowienieDetailsDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(order);
        }
        public async Task<IActionResult> ShowMyOrders()
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }

            var userId = _userManager.GetUserId(User);

            var response = await _httpClient.GetAsync($"http://localhost:7000/api/orders/user/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie udało się pobrać zamówień.";
                return RedirectToAction("Index", "Menu");
            }

            var json = await response.Content.ReadAsStringAsync();

            var zamowienia = JsonSerializer.Deserialize<List<ZamowienieDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(zamowienia);
        }

        public async Task<IActionResult> ShowAllOrders(int? KwerendaWyszukujaca)
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }

            HttpResponseMessage response;

            if (KwerendaWyszukujaca.HasValue)
            {
                response = await _httpClient.GetAsync(
                    $"http://localhost:7000/api/orders/search/{KwerendaWyszukujaca.Value}"
                );

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Nie udało się odnaleźć zamówienia o takim identyfikatorze!";
                    return RedirectToAction("ShowAllOrders");
                }

                var json = await response.Content.ReadAsStringAsync();

                // Deserializujemy POJEDYNCZE zamówienie
                var zamowienie = JsonSerializer.Deserialize<ZamowienieDto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Widok oczekuje listy → tworzymy listę z jednym elementem
                return View(new List<ZamowienieDto> { zamowienie });
            }
            else
            {
                response = await _httpClient.GetAsync(
                    $"http://localhost:7000/api/orders/all"
                );

                if (!response.IsSuccessStatusCode)
                {
                    return Forbid();
                }

                var json = await response.Content.ReadAsStringAsync();

                // 🔥 tutaj API zwraca listę → deserializujemy listę
                var zamowienia = JsonSerializer.Deserialize<List<ZamowienieDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View(zamowienia);
            }
        }
    }
}
