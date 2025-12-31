using BistroBossAPI.Services; // Dodano using dla ProductService
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace BistroBossAPI.Controllers
{
    public class BaseController : Controller
    {        
        // Wstrzykujemy Serwis
        protected readonly ProductService _productService;
        protected readonly BasketService _basketService;
        protected readonly CheckoutService _checkoutService;
        protected readonly OrderService _orderService;

        // Konstruktor przyjmuje Serwis
        public BaseController(ProductService productService, BasketService basketService, 
            CheckoutService checkoutService, OrderService orderService)
        {
            _productService = productService;
            _basketService = basketService;
            _checkoutService = checkoutService;
            _orderService = orderService;
        }

        // Zmieniamy metodę, aby była asynchroniczna i używała Serwisu
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Pobieranie kategorii z Serwisu
            var kategorie = await _productService.GetKategorieAsync();
            ViewData["Kategorie"] = kategorie.OrderBy(k => k.Nazwa).ToList();

            await base.OnActionExecutionAsync(context, next);
        }
    }
}