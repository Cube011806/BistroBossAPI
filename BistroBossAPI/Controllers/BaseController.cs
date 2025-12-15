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

        // Konstruktor przyjmuje Serwis
        public BaseController(ProductService productService)
        {
            _productService = productService;
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