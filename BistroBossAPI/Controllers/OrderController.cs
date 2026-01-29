using BistroBossAPI.Controllers.ApiControllers;
using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
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
            ManageService manageService,
            IConfiguration configuration)
            : base(productService, basketService, checkoutService, orderService, manageService)
        {
            _httpClient = httpClient;
            _userManager = userManager;
            _configuration = configuration;
        }
        private async Task SetJwtAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }
        }

        public async Task<IActionResult> ShowOrder(int id)
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var token = user.GenerateJwtToken(_configuration);
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }
                ViewBag.IsGuest = "0";
            }

            var response = await _httpClient.GetAsync($"http://localhost:7000/api/orders/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie można wyświetlić zamówienia.";
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

                var zamowienie = JsonSerializer.Deserialize<ZamowienieDto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

                var zamowienia = JsonSerializer.Deserialize<List<ZamowienieDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View(zamowienia);
            }
        }
        [HttpPost]
        public async Task<IActionResult> ReOrder(int id, bool sposobDostawy, string Miejscowosc, string Ulica, string NumerBudynku, string KodPocztowy)
        {
            await SetJwtAsync();

            var body = new
            {
                sposobDostawy,
                Miejscowosc,
                Ulica,
                NumerBudynku,
                KodPocztowy
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"http://localhost:7000/api/orders/reorder/{id}", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Żeby ponownie coś zamówić, nie możesz mieć zamówienia aktualnie w realizacji!";
                return RedirectToAction("ShowOrder", new { id });
            }

            var result = JsonSerializer.Deserialize<Dictionary<string, int>>(await response.Content.ReadAsStringAsync());
            var newId = result["id"];

            TempData["SuccessMessage"] = "Zamówienie zostało ponowione!";
            return RedirectToAction("ShowOrder", new { id = newId });
        }
        public IActionResult AddReview(int ZamowienieId)
        {
            var model = new AddReviewDto
            {
                ZamowienieId = ZamowienieId,
                Ocena = 1
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddReview(AddReviewDto opinia)
        {
            await SetJwtAsync();

            var json = JsonSerializer.Serialize(opinia);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://localhost:7000/api/orders/review", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = error;
                return RedirectToAction("AddReview", new { opinia.ZamowienieId });
            }

            TempData["SuccessMessage"] = "Pomyślnie dodano opinię!";
            return RedirectToAction("ShowOrder", new { id = opinia.ZamowienieId });
        }

        public async Task<IActionResult> CancelMyOrder(int id)
        {
            await SetJwtAsync();

            var response = await _httpClient.PutAsync($"http://localhost:7000/api/orders/{id}/cancel", null);

            return RedirectToAction("ShowOrder", new { id });
        }

    }
}
