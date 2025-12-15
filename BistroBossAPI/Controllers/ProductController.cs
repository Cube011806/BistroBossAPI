using BistroBossAPI.Data;
using BistroBossAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BistroBossAPI.Controllers
{
    public class ProductController : BaseController
    {

        private readonly UserManager<Uzytkownik> _userManager;

        public ProductController(ApplicationDbContext dbContext, UserManager<Uzytkownik> userManager) : base(dbContext)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Add()
        {
            UstawListeKategorii();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Produkt produkt, string? nowaKategoria, IFormFile? zdjeciePlik)
        {
            if (string.IsNullOrWhiteSpace(produkt.Nazwa) || string.IsNullOrWhiteSpace(produkt.Opis))
            {
                TempData["ErrorMessage"] = "Nazwa oraz opis produktu nie mogą być puste!";
                UstawListeKategorii();
                return View();
            }
            if (produkt.Cena <= 0)
            {
                TempData["ErrorMessage"] = "Cena produktu nie może być równa bądź mniejsza niż 0!";
                UstawListeKategorii();
                return View();
            }
            if (produkt.CzasPrzygotowania < 0)
            {
                TempData["ErrorMessage"] = "Czas przygotowania produktu nie może być mniejszy niż 0!";
                UstawListeKategorii();
                return View();
            }

            // Obsługa dodawania zdjęcia
            if (zdjeciePlik != null && zdjeciePlik.Length > 0)
            {
                var folderPath = Path.Combine("wwwroot", "images", "produkty");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(zdjeciePlik.FileName);
                var filePath = Path.Combine(folderPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await zdjeciePlik.CopyToAsync(stream);
                }

                produkt.Zdjecie = "/images/produkty/" + uniqueFileName;
            }

            // Dodanie nowej kategorii
            if (!string.IsNullOrWhiteSpace(nowaKategoria))
            {
                if (produkt.KategoriaId != 0)
                {
                    TempData["ErrorMessage"] = "Nie możesz wybrać kategorii z listy oraz podać nowej kategorii jednocześnie!";
                    UstawListeKategorii();
                    return View();
                }

                if (_dbContext.Kategorie.Any(k => k.Nazwa == nowaKategoria))
                {
                    TempData["ErrorMessage"] = "Wpisana nazwa kategorii produktu znajduje się już na liście kategorii!";
                    UstawListeKategorii();
                    return View();
                }

                var nowaKategoriaDoDodania = new Kategoria { Nazwa = nowaKategoria };
                _dbContext.Kategorie.Add(nowaKategoriaDoDodania);
                await _dbContext.SaveChangesAsync();

                produkt.KategoriaId = nowaKategoriaDoDodania.Id;
                _dbContext.Produkty.Add(produkt);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Produkt został pomyślnie dodany!";
                return RedirectToAction("Index", "Menu");
            }

            if (produkt.KategoriaId == 0)
            {
                TempData["ErrorMessage"] = "Musisz wybrać kategorię z listy bądź dodać nową kategorię produktu!";
                UstawListeKategorii();
                return View();
            }

            // Kategoria z listy
            _dbContext.Produkty.Add(produkt);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Produkt został pomyślnie dodany!";
            return RedirectToAction("Index", "Menu");
        }

        private void UstawListeKategorii()
        {
            var kategorie = _dbContext.Kategorie.ToList();
            var listaKategorii = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "-- Wybierz kategorię produktu --",
                    Selected = true
                }
            };
            listaKategorii.AddRange(kategorie.Select(k => new SelectListItem
            {
                Value = k.Id.ToString(),
                Text = k.Nazwa
            }));
            ViewBag.ListaKategorii = listaKategorii;
        }



    }
}
