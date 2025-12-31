using BistroBossAPI.Controllers;
using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

public class CheckoutController : BaseController
{
    private readonly UserManager<Uzytkownik> _userManager;
    private readonly HttpClient _httpClient;

    public CheckoutController(UserManager<Uzytkownik> userManager, HttpClient httpClient,
        ProductService productService, BasketService basketService,
        CheckoutService checkoutService, OrderService orderService)
        : base(productService, basketService, checkoutService, orderService)
    {
        _userManager = userManager;
        _httpClient = httpClient;
    }

    public async Task<IActionResult> Index()
    {
        string userId = User.Identity.IsAuthenticated
            ? _userManager.GetUserId(User)
            : "GUEST";

        var response = await _httpClient.GetAsync(
            $"http://localhost:7000/api/checkout/{userId}"
        );

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Nie udało się pobrać danych do zamówienia.";
            return RedirectToAction("Index", "Menu");
        }

        var json = await response.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<ZamowienieAddDto>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitOrder(ZamowienieAddDto dto)
    {
        bool isGuest = !User.Identity.IsAuthenticated;

        dto.UserId = isGuest
            ? "GUEST"
            : _userManager.GetUserId(User);

        // Jeśli gość → wysyłamy koszyk z session do api
        if (isGuest)
        {
            var jsonBasket = HttpContext.Session.GetString("basket");

            KoszykGuestDto basket = jsonBasket == null
                ? new KoszykGuestDto()
                : JsonSerializer.Deserialize<KoszykGuestDto>(jsonBasket);

            var basketJson = JsonSerializer.Serialize(basket);
            var basketContent = new StringContent(basketJson, Encoding.UTF8, "application/json");

            var basketResponse = await _httpClient.PostAsync(
                "http://localhost:7000/api/baskets/guest",
                basketContent
            );

            if (!basketResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Nie udało się przygotować koszyka gościa!";
                return RedirectToAction("Index");
            }
        }

        // Zamowienie
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("http://localhost:7000/api/orders", content);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Nie udało się złożyć zamówienia!";
            return RedirectToAction("Index");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var zamowienie = JsonSerializer.Deserialize<ZamowienieDto>(responseJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Czyścimy koszyk z sesji (pusty koszyk po złożżeniu zamówienia)
        HttpContext.Session.Remove("basket");

        TempData["SuccessMessage"] = "Zamówienie zostało złożone, dziękujemy! Numer zamówienia: " + zamowienie.Id;
        return RedirectToAction("ShowOrder", "Order", new { id = zamowienie.Id });
    }
}
