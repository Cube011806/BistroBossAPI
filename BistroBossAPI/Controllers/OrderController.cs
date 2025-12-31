using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BistroBossAPI.Controllers
{
    public class OrderController : BaseController
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<Uzytkownik> _userManager;

        public OrderController(
            HttpClient httpClient,
            UserManager<Uzytkownik> userManager,
            ProductService productService,
            BasketService basketService,
            CheckoutService checkoutService,
            OrderService orderService)
            : base(productService, basketService, checkoutService, orderService)
        {
            _httpClient = httpClient;
            _userManager = userManager;
        }

        public async Task<IActionResult> ShowOrder(int id)
        {
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
    }
}
