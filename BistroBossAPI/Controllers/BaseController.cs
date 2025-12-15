using BistroBossAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BistroBossAPI.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _dbContext;
        public BaseController(ApplicationDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var kategorie = _dbContext.Kategorie.OrderBy(k => k.Nazwa).ToList();
            ViewData["Kategorie"] = kategorie;

            base.OnActionExecuting(context);
        }
    }
}
