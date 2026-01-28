using BistroBossAPI.Controllers;
using BistroBossAPI.Controllers.ApiControllers;
using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

public class BasketController : BaseController
{
    private readonly UserManager<Uzytkownik> _userManager;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public BasketController(
        ProductService productService,
        BasketService basketService,
        CheckoutService checkoutService,
        OrderService orderService,
        ManageService manageService,
        UserManager<Uzytkownik> userManager,
        HttpClient httpClient,
        IConfiguration configuration)
        : base(productService, basketService, checkoutService, orderService, manageService)
    {
        _userManager = userManager;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
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

            return View("IndexGuest", basket);
        }

        var userId = _userManager.GetUserId(User);

        var response = await _httpClient.GetAsync($"http://localhost:7000/api/baskets/{userId}");

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Nie udało się pobrać koszyka.";
            return RedirectToAction("Index", "Menu");
        }

        var jsonLogged = await response.Content.ReadAsStringAsync();
        var basketLogged = JsonSerializer.Deserialize<KoszykDto>(jsonLogged,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View("IndexLoggedIn", basketLogged);
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

        if (!response.IsSuccessStatusCode)
            TempData["ErrorMessage"] = "Nie udało się dodać produktu do koszyka!";

        return RedirectToAction("Index", "Menu");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromBasket(int id)
    {
        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
        }

        var userId = _userManager.GetUserId(User);

        var response = await _httpClient.DeleteAsync(
            $"http://localhost:7000/api/baskets/{userId}/products/{id}"
        );

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            string message = doc.RootElement.GetProperty("message").GetString();

            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = "Nie udało się usunąć produktu z koszyka!";
        }

        return RedirectToAction("Index");
    }


    [HttpPost]
    public async Task<IActionResult> RemoveFromBasketGuest(int produktId)
    {
        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GenerateJwtToken(_configuration));
        }

        var json = HttpContext.Session.GetString("basket");

        if (json == null)
            return RedirectToAction("Index");

        var basket = JsonSerializer.Deserialize<KoszykGuestDto>(json);

        var item = basket.KoszykProdukty.FirstOrDefault(p => p.ProduktId == produktId);

        if (item != null)
        {
            if (item.Ilosc > 1)
            {
                item.Ilosc--;
                TempData["SuccessMessage"] = "Usunięto sztukę produktu z koszyka!";
            }
            else
            {
                basket.KoszykProdukty.Remove(item);
                TempData["SuccessMessage"] = "Usunięto produkt z koszyka!";
            }
        }

        HttpContext.Session.SetString("basket", JsonSerializer.Serialize(basket));

        return RedirectToAction("Index");
    }
}
