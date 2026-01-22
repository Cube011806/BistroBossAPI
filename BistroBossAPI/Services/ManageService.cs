using BistroBossAPI.Data;
using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;

namespace BistroBossAPI.Services
{
    public class ManageService
    {
        private readonly ApplicationDbContext _db;
        //private readonly IEmailService _emailService;

        public ManageService(ApplicationDbContext db)//, IEmailService emailService)
        {
            _db = db;
            //_emailService = emailService;
        }

        public async Task<List<ZamowienieListDto>> GetAllOrdersAsync(int? search)
        {
            IQueryable<Zamowienie> query = _db.Zamowienia;

            if (search.HasValue)
                query = query.Where(z => z.Id == search.Value);

            return await query
                .OrderBy(z => z.Status)
                .Select(z => new ZamowienieListDto
                {
                    Id = z.Id,
                    DataZamowienia = z.DataZamowienia,
                    CenaCalkowita = z.CenaCalkowita,
                    Status = z.Status,
                    SposobDostawy = z.SposobDostawy
                })
                .ToListAsync();
        }

        public async Task<ZamowienieDetailsDto?> GetOrderAsync(int id)
        {
            var z = await _db.Zamowienia
                .Include(z => z.ZamowioneProdukty)
                    .ThenInclude(zp => zp.Produkt)
                        .ThenInclude(p => p.Kategoria)
                .Include(z => z.Opinia)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (z == null) return null;

            return new ZamowienieDetailsDto
            {
                Id = z.Id,
                DataZamowienia = z.DataZamowienia,
                CenaCalkowita = z.CenaCalkowita,
                PrzewidywanyCzasRealizacji = z.PrzewidywanyCzasRealizacji,
                Status = z.Status,
                SposobDostawy = z.SposobDostawy,

                Imie = z.Imie,
                Nazwisko = z.Nazwisko,
                Email = z.Email,
                NumerTelefonu = z.NumerTelefonu,

                Miejscowosc = z.Miejscowosc,
                Ulica = z.Ulica,
                NumerBudynku = z.NumerBudynku,
                KodPocztowy = z.KodPocztowy,

                ZamowienieProdukty = z.ZamowioneProdukty.Select(zp => new ZamowienieProduktDto
                {
                    Ilosc = zp.Ilosc,
                    KategoriaNazwa = zp.Produkt.Kategoria.Nazwa,
                    Produkt = new ProduktDto
                    {
                        Id = zp.Produkt.Id,
                        Nazwa = zp.Produkt.Nazwa,
                        Cena = zp.Produkt.Cena,
                        Opis = zp.Produkt.Opis,
                        Zdjecie = zp.Produkt.Zdjecie
                    }
                }).ToList(),

                Opinia = z.Opinia == null ? null : new OpiniaDto
                {
                    Id = z.Opinia.Id,
                    Komentarz = z.Opinia.Komentarz,
                    Ocena = z.Opinia.Ocena
                }


            };
        }

        public async Task<AdminActionResultDto> SetStatusAsync(int id, int status)
        {
            var z = await _db.Zamowienia.FindAsync(id);
            if (z == null)
                return new AdminActionResultDto { Success = false, Message = "Zamówienie nie istnieje" };

            z.Status = status;
            await _db.SaveChangesAsync();

            return new AdminActionResultDto { Success = true, Message = "Status zmieniony" };
        }

        public async Task<AdminActionResultDto> SetInDeliveryAsync(int id)
        {
            var z = await _db.Zamowienia.FindAsync(id);
            if (z == null)
                return new AdminActionResultDto { Success = false, Message = "Zamówienie nie istnieje" };

            z.Status = 3;

            string emailTitle = z.SposobDostawy ? "Zamówienie w drodze" : "Gotowe do odbioru";
            string emailBody = z.SposobDostawy
                ? "<p>Twoje zamówienie jest w drodze!</p>"
                : "<p>Twoje zamówienie jest gotowe do odbioru!</p>";

            //_emailService.SendEmail(z.Email, emailTitle, emailBody);

            await _db.SaveChangesAsync();

            return new AdminActionResultDto { Success = true, Message = "Status zmieniony" };
        }

        public async Task<AdminActionResultDto> DeleteOrderAsync(int id)
        {
            var z = await _db.Zamowienia.FindAsync(id);
            if (z == null)
                return new AdminActionResultDto { Success = false, Message = "Zamówienie nie istnieje" };

            _db.Zamowienia.Remove(z);
            await _db.SaveChangesAsync();

            return new AdminActionResultDto { Success = true, Message = "Usunięto zamówienie" };
        }

        public async Task<List<UzytkownikListDto>> GetUsersAsync()
        {
            return await _db.Uzytkownicy
                .Where(u => u.Email != null && u.Id != "40000000")
                .Select(u => new UzytkownikListDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Imie = u.Imie,
                    Nazwisko = u.Nazwisko,
                    AccessLevel = u.AccessLevel
                })
                .ToListAsync();
        }

        public async Task<AdminActionResultDto> SetAdminAsync(string id, bool isAdmin)
        {
            var u = await _db.Uzytkownicy.FindAsync(id);
            if (u == null)
                return new AdminActionResultDto { Success = false, Message = "Użytkownik nie istnieje" };

            u.AccessLevel = isAdmin ? 1 : 0;
            await _db.SaveChangesAsync();

            return new AdminActionResultDto { Success = true, Message = "Zmieniono rangę" };
        }

        public async Task<AdminActionResultDto> RemoveUserAsync(string id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null)
                return new AdminActionResultDto { Success = false, Message = "Użytkownik nie istnieje" };

            _db.Users.Remove(u);
            await _db.SaveChangesAsync();

            return new AdminActionResultDto { Success = true, Message = "Usunięto użytkownika" };
        }
    }
}
