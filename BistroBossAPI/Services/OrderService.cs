using BistroBossAPI.Data;
using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace BistroBossAPI.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool Success, ZamowienieDto? Zamowienie, string ErrorMessage)>CreateOrderAsync(ZamowienieAddDto dto)
        {
            var koszyk = await _dbContext.Koszyki
                .Include(k => k.KoszykProdukty)
                    .ThenInclude(kp => kp.Produkt)
                .FirstOrDefaultAsync(k => k.UzytkownikId == dto.UserId);

            if (koszyk == null || !koszyk.KoszykProdukty.Any())
                return (false, null, "Koszyk jest pusty!");

            float cenaCalkowita = koszyk.KoszykProdukty.Sum(kp => kp.Produkt.Cena * kp.Ilosc);
            int czasMax = koszyk.KoszykProdukty.Max(kp => kp.Produkt.CzasPrzygotowania);

            var zamowienie = new Zamowienie
            {
                UzytkownikId = dto.UserId,
                DataZamowienia = DateTime.Now,
                CenaCalkowita = cenaCalkowita,
                PrzewidywanyCzasRealizacji = czasMax,
                Status = 1,

                Imie = dto.Imie,
                Nazwisko = dto.Nazwisko,
                Email = dto.Email,
                NumerTelefonu = dto.NumerTelefonu,

                Miejscowosc = dto.SposobDostawy ? dto.Miejscowosc : "",
                Ulica = dto.SposobDostawy ? dto.Ulica : "",
                NumerBudynku = dto.SposobDostawy ? dto.NumerBudynku : "",
                KodPocztowy = dto.SposobDostawy ? dto.KodPocztowy : "",
                SposobDostawy = dto.SposobDostawy
            };

            _dbContext.Zamowienia.Add(zamowienie);
            await _dbContext.SaveChangesAsync();

            foreach (var kp in koszyk.KoszykProdukty)
            {
                _dbContext.ZamowieniaProdukty.Add(new ZamowienieProdukt
                {
                    ZamowienieId = zamowienie.Id,
                    ProduktId = kp.ProduktId,
                    Ilosc = kp.Ilosc,
                    Cena = kp.Produkt.Cena
                });
            }

            _dbContext.KoszykProdukty.RemoveRange(koszyk.KoszykProdukty);
            await _dbContext.SaveChangesAsync();

            var dtoOut = new ZamowienieDto
            {
                Id = zamowienie.Id,
                DataZamowienia = zamowienie.DataZamowienia,
                CenaCalkowita = zamowienie.CenaCalkowita,
                PrzewidywanyCzasRealizacji = zamowienie.PrzewidywanyCzasRealizacji,
                SposobDostawy = zamowienie.SposobDostawy,

                Imie = zamowienie.Imie,
                Nazwisko = zamowienie.Nazwisko,
                Email = zamowienie.Email,
                NumerTelefonu = zamowienie.NumerTelefonu,
                Miejscowosc = zamowienie.Miejscowosc,
                Ulica = zamowienie.Ulica,
                NumerBudynku = zamowienie.NumerBudynku,
                KodPocztowy = zamowienie.KodPocztowy
            };

            return (true, dtoOut, "");
        }

        public async Task<(bool Success, ZamowienieDetailsDto? Zamowienie, string ErrorMessage)>GetOrderAsync(int id)
        {
            var zamowienie = await _dbContext.Zamowienia
                .Include(z => z.ZamowioneProdukty)
                    .ThenInclude(zp => zp.Produkt)
                        .ThenInclude(p => p.Kategoria)
                .Include(z => z.Opinia)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zamowienie == null)
                return (false, null, "Nie znaleziono zamówienia.");

            var dto = new ZamowienieDetailsDto
            {
                Id = zamowienie.Id,
                DataZamowienia = zamowienie.DataZamowienia,
                CenaCalkowita = zamowienie.CenaCalkowita,
                PrzewidywanyCzasRealizacji = zamowienie.PrzewidywanyCzasRealizacji,
                Status = zamowienie.Status,
                SposobDostawy = zamowienie.SposobDostawy,

                Imie = zamowienie.Imie,
                Nazwisko = zamowienie.Nazwisko,
                Email = zamowienie.Email,
                NumerTelefonu = zamowienie.NumerTelefonu,

                Miejscowosc = zamowienie.Miejscowosc,
                Ulica = zamowienie.Ulica,
                NumerBudynku = zamowienie.NumerBudynku,
                KodPocztowy = zamowienie.KodPocztowy,

                ZamowienieProdukty = zamowienie.ZamowioneProdukty.Select(zp => new ZamowienieProduktDto
                {
                    Ilosc = zp.Ilosc,
                    Produkt = new ProduktDto
                    {
                        Id = zp.Produkt.Id,
                        Nazwa = zp.Produkt.Nazwa,
                        Opis = zp.Produkt.Opis,
                        Cena = zp.Produkt.Cena,
                        CzasPrzygotowania = zp.Produkt.CzasPrzygotowania,
                        Zdjecie = zp.Produkt.Zdjecie,
                        KategoriaId = zp.Produkt.KategoriaId
                    },
                    KategoriaNazwa = zp.Produkt.Kategoria.Nazwa
                }).ToList(),

                Opinia = zamowienie.Opinia == null ? null : new OpiniaDto
                {
                    Ocena = zamowienie.Opinia.Ocena,
                    Komentarz = zamowienie.Opinia.Komentarz
                }
            };

            return (true, dto, "");
        }

        public async Task<List<ZamowienieDto>> GetOrdersForUserAsync(string userId)
        {
            var zamowienia = await _dbContext.Zamowienia
                .Where(z => z.UzytkownikId == userId)
                .OrderByDescending(z => z.Id)
                .ToListAsync();

            return zamowienia.Select(z => new ZamowienieDto
            {
                Id = z.Id,
                DataZamowienia = z.DataZamowienia,
                CenaCalkowita = z.CenaCalkowita,
                PrzewidywanyCzasRealizacji = z.PrzewidywanyCzasRealizacji,
                SposobDostawy = z.SposobDostawy,
                Status = z.Status,

                Imie = z.Imie,
                Nazwisko = z.Nazwisko,
                Email = z.Email,
                NumerTelefonu = z.NumerTelefonu,

                Miejscowosc = z.Miejscowosc,
                Ulica = z.Ulica,
                NumerBudynku = z.NumerBudynku,
                KodPocztowy = z.KodPocztowy

            }).ToList();
        }

        public async Task<List<ZamowienieDto>> GetAllOrdersAsync()
        {
            var zamowienia = await _dbContext.Zamowienia
                .OrderBy(z => z.Status)
                .ToListAsync();

            return zamowienia.Select(z => new ZamowienieDto
            {
                Id = z.Id,
                DataZamowienia = z.DataZamowienia,
                CenaCalkowita = z.CenaCalkowita,
                PrzewidywanyCzasRealizacji = z.PrzewidywanyCzasRealizacji,
                SposobDostawy = z.SposobDostawy,
                Status = z.Status,

                Imie = z.Imie,
                Nazwisko = z.Nazwisko,
                Email = z.Email,
                NumerTelefonu = z.NumerTelefonu,

                Miejscowosc = z.Miejscowosc,
                Ulica = z.Ulica,
                NumerBudynku = z.NumerBudynku,
                KodPocztowy = z.KodPocztowy
            }).ToList();
        }

    }
}
