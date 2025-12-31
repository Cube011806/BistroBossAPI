using BistroBossAPI.Data;
using BistroBossAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace BistroBossAPI.Services
{
    public class CheckoutService
    {
        private readonly ApplicationDbContext _db;

        public CheckoutService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ZamowienieAddDto> GetCheckoutDataAsync(string userId)
        {
            var user = await _db.Uzytkownicy.FirstOrDefaultAsync(u => u.Id == userId);

            return new ZamowienieAddDto
            {
                UserId = userId,
                Imie = user?.Imie ?? "",
                Nazwisko = user?.Nazwisko ?? "",
                Email = user?.Email ?? "",
                NumerTelefonu = user?.PhoneNumber ?? "",
                IsGuest = user == null
            };
        }
    }
}
