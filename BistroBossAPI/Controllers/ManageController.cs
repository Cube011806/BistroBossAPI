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
    public class ManageController : BaseController
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<Uzytkownik> _userManager;
        private readonly IConfiguration _configuration;

        public ManageController(HttpClient httpClient, UserManager<Uzytkownik> userManager,
            ProductService productService, BasketService basketService,
            CheckoutService checkoutService, OrderService orderService, ManageService manageService, IConfiguration configuration)
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

        public async Task<IActionResult> ShowAllOrders(int? search)
        {
            await SetJwtAsync();

            var url = "http://localhost:7000/api/manage/orders";
            if (search.HasValue)
                url += $"?search={search.Value}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie udało się pobrać zamówień!";
                return View(new List<ZamowienieListDto>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var zamowienia = JsonSerializer.Deserialize<List<ZamowienieListDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(zamowienia);
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ShowOrder(int id)
        {
            await SetJwtAsync();

            var response = await _httpClient.GetAsync($"http://localhost:7000/api/manage/orders/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie udało się pobrać szczegółów zamówienia!";
                return RedirectToAction("ShowAllOrders");
            }

            var json = await response.Content.ReadAsStringAsync();
            var zamowienie = JsonSerializer.Deserialize<ZamowienieDetailsDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(zamowienie);
        }

        public async Task<IActionResult> SetCancelled(int id)
        {
            await SetJwtAsync();
            await _httpClient.PutAsync($"http://localhost:7000/api/manage/orders/{id}/cancel", null);
            return RedirectToAction("ShowOrder", new { id });
        }

        public async Task<IActionResult> SetInPreparation(int id)
        {
            await SetJwtAsync();
            await _httpClient.PutAsync($"http://localhost:7000/api/manage/orders/{id}/prepare", null);
            return RedirectToAction("ShowOrder", new { id });
        }

        public async Task<IActionResult> SetInDelivery(int id)
        {
            await SetJwtAsync();
            await _httpClient.PutAsync($"http://localhost:7000/api/manage/orders/{id}/delivery", null);
            return RedirectToAction("ShowOrder", new { id });
        }

        public async Task<IActionResult> SetCompleted(int id)
        {
            await SetJwtAsync();
            await _httpClient.PutAsync($"http://localhost:7000/api/manage/orders/{id}/complete", null);
            return RedirectToAction("ShowOrder", new { id });
        }

        public async Task<IActionResult> DeleteOrder(int id)
        {
            await SetJwtAsync();
            await _httpClient.DeleteAsync($"http://localhost:7000/api/manage/orders/{id}");
            return RedirectToAction("ShowAllOrders");
        }

        public async Task<IActionResult> ShowUsers()
        {
            await SetJwtAsync();

            var response = await _httpClient.GetAsync("http://localhost:7000/api/manage/users");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie udało się pobrać użytkowników!";
                return View(new List<UzytkownikListDto>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UzytkownikListDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(users);
        }

        public async Task<IActionResult> MakeAdmin(string id)
        {
            await SetJwtAsync();
            await _httpClient.PutAsync($"http://localhost:7000/api/manage/users/{id}/admin", null);
            return RedirectToAction("ShowUsers");
        }

        public async Task<IActionResult> UnmakeAdmin(string id)
        {
            await SetJwtAsync();
            await _httpClient.PutAsync($"http://localhost:7000/api/manage/users/{id}/unadmin", null);
            return RedirectToAction("ShowUsers");
        }

        public async Task<IActionResult> RemoveUser(string id)
        {
            await SetJwtAsync();
            await _httpClient.DeleteAsync($"http://localhost:7000/api/manage/users/{id}");
            return RedirectToAction("ShowUsers");
        }
    }
}
