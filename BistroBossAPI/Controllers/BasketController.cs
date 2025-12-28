using BistroBossAPI.Controllers;
using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

public class BasketController : BaseController
{
    private readonly UserManager<Uzytkownik> _userManager;
    private readonly HttpClient _httpClient;

    public BasketController(
        ProductService productService,
        BasketService basketService,
        UserManager<Uzytkownik> userManager,
        HttpClient httpClient)
        : base(productService, basketService)
    {
        _userManager = userManager;
        _httpClient = httpClient;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
        {
            TempData["ErrorMessage"] = "Musisz być zalogowany, aby zobaczyć koszyk.";
            return RedirectToAction("Index", "Menu");
        }

        var response = await _httpClient.GetAsync($"http://localhost:7000/api/baskets/{userId}");

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Nie udało się pobrać koszyka.";
            return RedirectToAction("Index", "Menu");
        }

        var json = await response.Content.ReadAsStringAsync();
        var basket = JsonSerializer.Deserialize<KoszykDto>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View("IndexLoggedIn", basket);
    }
}
