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

        public async Task<ProduktDto?> GetProductByIdAsync(int id)
        {
            var produkt = await _dbContext.Produkty.FirstOrDefaultAsync(p => p.Id == id);

            if (produkt == null)
                return null;

            return new ProduktDto
            {
                Id = produkt.Id,
                Nazwa = produkt.Nazwa,
                Opis = produkt.Opis,
                Cena = produkt.Cena,
                CzasPrzygotowania = produkt.CzasPrzygotowania,
                Zdjecie = produkt.Zdjecie,
                KategoriaId = produkt.KategoriaId
            };
        }

        //Pobranie produktów z menu
        public async Task<List<KategoriaMenuDto>> GetMenuDtoAsync()
        {
            var kategorie = await _dbContext.Kategorie
                .Include(k => k.Produkty)
                .ToListAsync();

            return kategorie.Select(k => new KategoriaMenuDto
            {
                Id = k.Id,
                Nazwa = k.Nazwa,
                Produkty = k.Produkty.Select(p => new ProduktMenuDto
                {
                    Id = p.Id,
                    Nazwa = p.Nazwa,
                    Cena = p.Cena,
                    CzasPrzygotowania = p.CzasPrzygotowania,
                    Zdjecie = p.Zdjecie
                }).ToList()
            }).ToList();
        }

        //Dodanie produktu
        public async Task<(bool Success, ProduktDto? Produkt, string ErrorMessage)> AddProductAsync(ProduktAddDto dto, string? nowaKategoria)
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

            var produktDto = new ProduktDto
            {
                Id = produkt.Id,
                Nazwa = produkt.Nazwa,
                Opis = produkt.Opis,
                Cena = produkt.Cena,
                CzasPrzygotowania = produkt.CzasPrzygotowania,
                Zdjecie = produkt.Zdjecie,
                KategoriaId = produkt.KategoriaId
            };

            return (true, produktDto, string.Empty);
        }

        //Edycja produktu
        public async Task<(bool Success, ProduktDto? Produkt, string ErrorMessage)> UpdateProductAsync(int id, ProduktEditDto dto, string? nowaKategoria)
        {
            var produkt = await _dbContext.Produkty.FirstOrDefaultAsync(p => p.Id == id);

            if (produkt == null)
                return (false, null, "Nie znaleziono produktu!");

            if (string.IsNullOrWhiteSpace(dto.Nazwa) || string.IsNullOrWhiteSpace(dto.Opis))
                return (false, null, "Nazwa oraz opis produktu nie mogą być puste!");

            if (dto.Cena <= 0)
                return (false, null, "Cena produktu nie może być równa bądź mniejsza niż 0!");

            if (dto.CzasPrzygotowania < 0)
                return (false, null, "Czas przygotowania produktu nie może być mniejszy niż 0!");

            int kategoriaId = dto.KategoriaId;

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

            produkt.Nazwa = dto.Nazwa;
            produkt.Opis = dto.Opis;
            produkt.Cena = dto.Cena;
            produkt.CzasPrzygotowania = dto.CzasPrzygotowania;
            if (!string.IsNullOrWhiteSpace(dto.Zdjecie))
            {
                produkt.Zdjecie = dto.Zdjecie;
            }
            produkt.KategoriaId = kategoriaId;

            await _dbContext.SaveChangesAsync();

            var produktDto = new ProduktDto
            {
                Id = produkt.Id,
                Nazwa = produkt.Nazwa,
                Opis = produkt.Opis,
                Cena = produkt.Cena,
                CzasPrzygotowania = produkt.CzasPrzygotowania,
                Zdjecie = produkt.Zdjecie,
                KategoriaId = produkt.KategoriaId
            };

            return (true, produktDto, string.Empty);
        }
    }
}