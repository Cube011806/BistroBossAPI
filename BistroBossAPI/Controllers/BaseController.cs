using BistroBossAPI.Services; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace BistroBossAPI.Controllers
{
    public class BaseController : Controller
    {        
        protected readonly ProductService _productService;
        protected readonly BasketService _basketService;
        protected readonly CheckoutService _checkoutService;
        protected readonly OrderService _orderService;
        protected readonly ManageService _manageService;

        public BaseController(ProductService productService, BasketService basketService, 
            CheckoutService checkoutService, OrderService orderService, ManageService manageService)
        {
            _productService = productService;
            _basketService = basketService;
            _checkoutService = checkoutService;
            _orderService = orderService;
            _manageService = manageService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var kategorie = await _productService.GetKategorieAsync();
            ViewData["Kategorie"] = kategorie.OrderBy(k => k.Nazwa).ToList();

            await base.OnActionExecutionAsync(context, next);
        }
    }
}