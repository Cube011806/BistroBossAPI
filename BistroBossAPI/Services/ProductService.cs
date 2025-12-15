// Services/ProductService.cs 
using BistroBossAPI.Data;
using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BistroBossAPI.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Kategoria>> GetKategorieAsync()
        {
            return await _dbContext.Kategorie.ToListAsync();
        }

        public async Task<Produkt?> GetProductByIdAsync(int id)
        {
            return await _dbContext.Produkty
                .Include(p => p.Kategoria)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(bool Success, Produkt? Produkt, string ErrorMessage)> AddProductAsync(ProduktCreateDto dto, string? nowaKategoria)
        {
            // Walidacja danych
            if (string.IsNullOrWhiteSpace(dto.Nazwa) || string.IsNullOrWhiteSpace(dto.Opis))
                return (false, null, "Nazwa oraz opis produktu nie mogą być puste!");

            if (dto.Cena <= 0)
                return (false, null, "Cena produktu nie może być równa bądź mniejsza niż 0!");

            if (dto.CzasPrzygotowania < 0)
                return (false, null, "Czas przygotowania produktu nie może być mniejszy niż 0!");

            int kategoriaId = dto.KategoriaId;

            // Obsługa dodawania nowej kategorii
            if (!string.IsNullOrWhiteSpace(nowaKategoria))
            {
                if (dto.KategoriaId != 0)
                    return (false, null, "Nie możesz wybrać kategorii z listy oraz podać nowej kategorii jednocześnie!");

                if (await _dbContext.Kategorie.AnyAsync(k => k.Nazwa == nowaKategoria))
                    return (false, null, "Wpisana nazwa kategorii produktu znajduje się już na liście kategorii!");

                var nowa = new Kategoria { Nazwa = nowaKategoria };
                _dbContext.Kategorie.Add(nowa);
                await _dbContext.SaveChangesAsync();

                kategoriaId = nowa.Id;
            }
            else if (dto.KategoriaId == 0)
            {
                return (false, null, "Musisz wybrać kategorię z listy bądź dodać nową kategorię produktu!");
            }

            // Tworzenie produktu na podstawie DTO
            var produkt = new Produkt
            {
                Nazwa = dto.Nazwa,
                Opis = dto.Opis,
                Cena = dto.Cena,
                CzasPrzygotowania = dto.CzasPrzygotowania,
                Zdjecie = dto.Zdjecie,
                KategoriaId = kategoriaId
            };

            // Zapis do bazy
            _dbContext.Produkty.Add(produkt);
            await _dbContext.SaveChangesAsync();

            return (true, produkt, string.Empty);
        }
    }
}