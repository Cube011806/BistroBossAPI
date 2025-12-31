using System.Diagnostics;
using BistroBossAPI.Models;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BistroBossAPI.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ProductService productService, 
            BasketService basketService, CheckoutService checkoutService, OrderService orderService) 
            : base(productService, basketService, checkoutService, orderService)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
