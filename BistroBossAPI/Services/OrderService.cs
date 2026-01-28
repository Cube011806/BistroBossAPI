using BistroBossAPI.Data;
using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace BistroBossAPI.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly IEmailService _emailService;

        public OrderService(ApplicationDbContext dbContext, IEmailService emailService)
        {
            _dbContext = dbContext;
            _emailService = emailService;
        }

        public async Task<(bool Success, ZamowienieDto? Zamowienie, string ErrorMessage)> CreateOrderAsync(ZamowienieAddDto dto)
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

            string messageDelivery = $@"
            <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h2 style='color: #4CAF50;'>Dziękujemy za Twoje zamówienie!</h2>
                    <p><strong>Numer zamówienia:</strong> {zamowienie.Id}</p>
                    <p><strong>Data zamówienia:</strong> {zamowienie.DataZamowienia:dd.MM.yyyy HH:mm}</p>
                    <p><strong>Przewidywany czas realizacji:</strong> {zamowienie.PrzewidywanyCzasRealizacji} minut</p>
                    <p><strong>Cena całkowita:</strong> {zamowienie.CenaCalkowita} zł</p>
                    <p><strong>Adres dostawy:</strong><br />
                        {zamowienie.Miejscowosc}, {zamowienie.Ulica} {zamowienie.NumerBudynku}<br />
                        {zamowienie.KodPocztowy}
                    </p>
                    <hr style='margin: 20px 0;' />
                    <p>W razie pytań prosimy o kontakt z naszym działem obsługi klienta.</p>
                    <p style='color: #777;'>Pozdrawiamy,<br />Zespół BistroBoss</p>
                </body>
            </html>";

            string messagePickup = $@"
            <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h2 style='color: #4CAF50;'>Dziękujemy za Twoje zamówienie!</h2>
                    <p><strong>Numer zamówienia:</strong> {zamowienie.Id}</p>
                    <p><strong>Data zamówienia:</strong> {zamowienie.DataZamowienia:dd.MM.yyyy HH:mm}</p>
                    <p><strong>Przewidywany czas realizacji:</strong> {zamowienie.PrzewidywanyCzasRealizacji} minut</p>
                    <p><strong>Cena całkowita:</strong> {zamowienie.CenaCalkowita} zł</p>
                    <hr style='margin: 20px 0;' />
                    <p>W razie pytań prosimy o kontakt z naszym działem obsługi klienta.</p>
                    <p style='color: #777;'>Pozdrawiamy,<br />Zespół BistroBoss</p>
                </body>
            </html>";

            if (zamowienie.SposobDostawy)
                _emailService.SendEmail(zamowienie.Email, "Nowe zamówienie", messageDelivery);
            else
                _emailService.SendEmail(zamowienie.Email, "Nowe zamówienie", messagePickup);



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


        public async Task<bool> IsOrderForUser(string? userId)
        {
            if (userId == null)
            {
                return false;
            }

            return await _dbContext.Zamowienia
                .Where(z => z.UzytkownikId == userId)
                .FirstOrDefaultAsync() != null;
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
                UzytkownikId = zamowienie.UzytkownikId,
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
        public async Task<bool> HasActiveOrderAsync(string userId)
        {
            return await _dbContext.Zamowienia
                .AnyAsync(z => z.UzytkownikId == userId && z.Status != 4 && z.Status != 0);
        }

        public async Task<(bool Success, Zamowienie? Zamowienie, string? ErrorMessage)> GetOrderEntityAsync(int id)
        {
            var order = await _dbContext.Zamowienia
                .Include(z => z.ZamowioneProdukty)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (order == null)
                return (false, null, "Nie znaleziono zamówienia.");

            return (true, order, null);
        }

        public async Task<(bool Success, Zamowienie Zamowienie, string? ErrorMessage)> ReOrderAsync(
            Zamowienie oldOrder,
            string userId,
            ReOrderRequestDto dto)
        {
            var newOrder = new Zamowienie
            {
                UzytkownikId = userId,
                Status = 1,
                PrzewidywanyCzasRealizacji = oldOrder.PrzewidywanyCzasRealizacji,
                CenaCalkowita = oldOrder.CenaCalkowita,
                SposobDostawy = dto.SposobDostawy,
                Imie = oldOrder.Imie,
                Nazwisko = oldOrder.Nazwisko,
                Email = oldOrder.Email,
                NumerTelefonu = oldOrder.NumerTelefonu,
                Ulica = dto.SposobDostawy ? dto.Ulica : "",
                NumerBudynku = dto.SposobDostawy ? dto.NumerBudynku : "",
                Miejscowosc = dto.SposobDostawy ? dto.Miejscowosc : "",
                KodPocztowy = dto.SposobDostawy ? dto.KodPocztowy : "",
                DataZamowienia = DateTime.Now,
                ZamowioneProdukty = oldOrder.ZamowioneProdukty.Select(zp => new ZamowienieProdukt
                {
                    ProduktId = zp.ProduktId,
                    Ilosc = zp.Ilosc,
                    Cena = zp.Cena
                }).ToList()
            };

            _dbContext.Zamowienia.Add(newOrder);
            await _dbContext.SaveChangesAsync();

            string messageDelivery = $@"
            <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h2 style='color: #4CAF50;'>Dziękujemy za Twoje zamówienie!</h2>
                    <p><strong>Numer zamówienia:</strong> {newOrder.Id}</p>
                    <p><strong>Data zamówienia:</strong> {newOrder.DataZamowienia:dd.MM.yyyy HH:mm}</p>
                    <p><strong>Przewidywany czas realizacji:</strong> {newOrder.PrzewidywanyCzasRealizacji} minut</p>
                    <p><strong>Cena całkowita:</strong> {newOrder.CenaCalkowita} zł</p>
                    <p><strong>Adres dostawy:</strong><br />
                        {newOrder.Miejscowosc}, {newOrder.Ulica} {newOrder.NumerBudynku}<br />
                        {newOrder.KodPocztowy}
                    </p>
                    <hr style='margin: 20px 0;' />
                    <p>W razie pytań prosimy o kontakt z naszym działem obsługi klienta.</p>
                    <p style='color: #777;'>Pozdrawiamy,<br />Zespół BistroBoss</p>
                </body>
            </html>";

            string messagePickup = $@"
            <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h2 style='color: #4CAF50;'>Dziękujemy za Twoje zamówienie!</h2>
                    <p><strong>Numer zamówienia:</strong> {newOrder.Id}</p>
                    <p><strong>Data zamówienia:</strong> {newOrder.DataZamowienia:dd.MM.yyyy HH:mm}</p>
                    <p><strong>Przewidywany czas realizacji:</strong> {newOrder.PrzewidywanyCzasRealizacji} minut</p>
                    <p><strong>Cena całkowita:</strong> {newOrder.CenaCalkowita} zł</p>
                    <hr style='margin: 20px 0;' />
                    <p>W razie pytań prosimy o kontakt z naszym działem obsługi klienta.</p>
                    <p style='color: #777;'>Pozdrawiamy,<br />Zespół BistroBoss</p>
                </body>
            </html>";

            if (newOrder.SposobDostawy)
                _emailService.SendEmail(newOrder.Email, "Nowe zamówienie", messageDelivery);
            else
                _emailService.SendEmail(newOrder.Email, "Nowe zamówienie", messagePickup);

            return (true, newOrder, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> AddReviewAsync(AddReviewDto dto, string userId)
        {
            var order = await _dbContext.Zamowienia
                .Include(z => z.Opinia)
                .FirstOrDefaultAsync(z => z.Id == dto.ZamowienieId);

            if (order == null)
                return (false, "Nie znaleziono zamówienia.");

            if (order.UzytkownikId != userId)
                return (false, "Nie możesz dodać opinii do cudzego zamówienia.");

            if (order.Opinia != null)
                return (false, "To zamówienie ma już opinię.");

            var opinia = new Opinia
            {
                ZamowienieId = dto.ZamowienieId,
                UzytkownikId = userId,
                Ocena = dto.Ocena,
                Komentarz = dto.Komentarz
            };

            _dbContext.Opinie.Add(opinia);
            order.Opinia = opinia;

            await _dbContext.SaveChangesAsync();

            return (true, null);
        }

        public async Task UpdateOrderAsync(Zamowienie order)
        {
            _dbContext.Zamowienia.Update(order);
            await _dbContext.SaveChangesAsync();
        }


    }
}
