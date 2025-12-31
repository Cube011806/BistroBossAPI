using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BistroBossAPI.Controllers
{
    public class MenuController : BaseController
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<Uzytkownik> _userManager;

        public MenuController(HttpClient httpClient, UserManager<Uzytkownik> userManager, 
            ProductService productService, BasketService basketService, 
            CheckoutService checkoutService, OrderService orderService)
            : base(productService, basketService, checkoutService, orderService)
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
            // Gość
            if (!User.Identity.IsAuthenticated)
            {
                var json = HttpContext.Session.GetString("basket");

                KoszykGuestDto basket = json == null
                    ? new KoszykGuestDto()
                    : JsonSerializer.Deserialize<KoszykGuestDto>(json);

                // pobieramy produkt z ProductService (BaseController)
                var produkt = await _productService.GetProductByIdAsync(produktId);

                var existing = basket.KoszykProdukty.FirstOrDefault(p => p.ProduktId == produktId);

                if (existing == null)
                {
                    basket.KoszykProdukty.Add(new KoszykGuestProduktDto
                    {
                        ProduktId = produktId,
                        Ilosc = 1,
                        Produkt = produkt
                    });
                }
                else
                {
                    existing.Ilosc++;
                }

                HttpContext.Session.SetString("basket", JsonSerializer.Serialize(basket));

                TempData["SuccessMessage"] = "Produkt został dodany do koszyka!";
                return RedirectToAction("Index", "Menu");
            }

            // Zalogowany
            var userId = _userManager.GetUserId(User);

            var response = await _httpClient.PostAsync(
                $"http://localhost:7000/api/baskets?userId={userId}&produktId={produktId}",
                null
            );

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Produkt został dodany do koszyka!";
            else
                TempData["ErrorMessage"] = "Nie udało się dodać produktu do koszyka!";

            return RedirectToAction("Index", "Menu");
        }
    }
}
