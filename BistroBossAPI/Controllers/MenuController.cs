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
    public class MenuController : BaseController
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<Uzytkownik> _userManager;
        private readonly IConfiguration _configuration;

        public MenuController(HttpClient httpClient, UserManager<Uzytkownik> userManager, 
            ProductService productService, BasketService basketService, 
            CheckoutService checkoutService, OrderService orderService, ManageService manageService, IConfiguration configuration)
            : base(productService, basketService, checkoutService, orderService, manageService)
        {
            _httpClient = httpClient;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int? kategoriaId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }

            var response = await _httpClient.GetAsync("http://localhost:7000/api/products/menu");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie udało się pobrać menu!";
                return View(new List<KategoriaMenuDto>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var kategorie = JsonSerializer.Deserialize<List<KategoriaMenuDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (kategoriaId.HasValue)
            {
                var przefiltrowane = kategorie.Where(k => k.Id == kategoriaId.Value).ToList();
                return View(przefiltrowane);
            }

            return View(kategorie);
        }

        [HttpPost]
        public async Task<IActionResult> AddToBasket(int produktId)
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
            }

            if (!User.Identity.IsAuthenticated)
            {
                var json = HttpContext.Session.GetString("basket");

                KoszykGuestDto basket = json == null
                    ? new KoszykGuestDto()
                    : JsonSerializer.Deserialize<KoszykGuestDto>(json);

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
