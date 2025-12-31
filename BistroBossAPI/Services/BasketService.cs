using BistroBossAPI.Data;
using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace BistroBossAPI.Services
{
    public class BasketService
    {
        private readonly ApplicationDbContext _dbContext;

        public BasketService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<KoszykDto> GetBasketForUserAsync(string userId)
        {
            var koszyk = await _dbContext.Koszyki
                .Include(k => k.KoszykProdukty)
                .ThenInclude(kp => kp.Produkt)
                .FirstOrDefaultAsync(k => k.UzytkownikId == userId);

            if (koszyk == null)
            {
                koszyk = new Koszyk { UzytkownikId = userId };
                _dbContext.Koszyki.Add(koszyk);
                await _dbContext.SaveChangesAsync();
            }

            return new KoszykDto
            {
                Id = koszyk.Id,
                UzytkownikId = koszyk.UzytkownikId,
                KoszykProdukty = koszyk.KoszykProdukty.Select(kp => new KoszykProduktDto
                {
                    Id = kp.Id,
                    ProduktId = kp.ProduktId,
                    Nazwa = kp.Produkt.Nazwa,
                    Cena = kp.Produkt.Cena,
                    Ilosc = kp.Ilosc,
                    Zdjecie = kp.Produkt.Zdjecie
                }).ToList()
            };
        }
        public async Task<(bool Success, string ErrorMessage)> AddToBasketAsync(string userId, int produktId)
        {
            var produkt = await _dbContext.Produkty.FindAsync(produktId);
            if (produkt == null)
                return (false, "Nie znaleziono produktu.");

            var koszyk = await _dbContext.Koszyki
                .Include(k => k.KoszykProdukty)
                .FirstOrDefaultAsync(k => k.UzytkownikId == userId);

            if (koszyk == null)
            {
                koszyk = new Koszyk { UzytkownikId = userId };
                _dbContext.Koszyki.Add(koszyk);
                await _dbContext.SaveChangesAsync();
            }

            var koszykProdukt = koszyk.KoszykProdukty
                .FirstOrDefault(kp => kp.ProduktId == produktId);

            if (koszykProdukt != null)
            {
                koszykProdukt.Ilosc++;
            }
            else
            {
                var kp = new KoszykProdukt
                {
                    ProduktId = produktId,
                    Ilosc = 1,
                    KoszykId = koszyk.Id
                };

                _dbContext.KoszykProdukty.Add(kp);
            }

            await _dbContext.SaveChangesAsync();
            return (true, "");
        }
        public async Task<(bool Success, string Message, string ErrorMessage)> RemoveFromBasketAsync(string userId, int koszykProduktId)
        {
            var koszykProdukt = await _dbContext.KoszykProdukty
                .Include(kp => kp.Koszyk)
                .FirstOrDefaultAsync(kp => kp.Id == koszykProduktId && kp.Koszyk.UzytkownikId == userId);

            if (koszykProdukt == null)
                return (false, "", "Nie znaleziono produktu w koszyku.");

            if (koszykProdukt.Ilosc > 1)
            {
                koszykProdukt.Ilosc--;
                await _dbContext.SaveChangesAsync();
                return (true, "Usunięto sztukę produktu z koszyka!", "");
            }
            else
            {
                _dbContext.KoszykProdukty.Remove(koszykProdukt);
                await _dbContext.SaveChangesAsync();
                return (true, "Usunięto produkt z koszyka!", "");
            }
        }
        public async Task<bool> SetGuestBasketAsync(KoszykGuestDto dto)
        {
            // usuń stary koszyk gościa
            var old = await _dbContext.Koszyki
                .Include(k => k.KoszykProdukty)
                .Where(k => k.UzytkownikId == "GUEST")
                .FirstOrDefaultAsync();

            if (old != null)
            {
                _dbContext.KoszykProdukty.RemoveRange(old.KoszykProdukty);
                _dbContext.Koszyki.Remove(old);
                await _dbContext.SaveChangesAsync();
            }

            // utwórz nowy koszyk
            var koszyk = new Koszyk
            {
                UzytkownikId = "GUEST"
            };

            _dbContext.Koszyki.Add(koszyk);
            await _dbContext.SaveChangesAsync();

            foreach (var p in dto.KoszykProdukty)
            {
                _dbContext.KoszykProdukty.Add(new KoszykProdukt
                {
                    KoszykId = koszyk.Id,
                    ProduktId = p.ProduktId,
                    Ilosc = p.Ilosc
                });
            }

            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
