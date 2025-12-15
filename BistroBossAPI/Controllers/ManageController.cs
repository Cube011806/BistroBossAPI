using BistroBossAPI.Data;
using BistroBossAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BistroBossAPI.Controllers
{
    public class ManageController : BaseController
    {
        private readonly UserManager<Uzytkownik> _userManager;
        //email service może tutaj być w przyszłości

        public ManageController(ApplicationDbContext dbContext, UserManager<Uzytkownik> userManager) : base(dbContext)
        {
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
