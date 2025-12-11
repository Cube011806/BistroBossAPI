using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace BistroBossAPI.Models
{
    public class Zamowienie
    {
        public int Id { get; set; }
        public DateTime DataZamowienia { get; set; }
        public float CenaCalkowita { get; set; }
        public int PrzewidywanyCzasRealizacji { get; set; }
        //public int StatusId { get; set; }
        public int Status { get; set; } = 0; // 1 -złożono 2- w przygotowaniu 3 - w dostawie 4 - zrealizowano 3 - gotowe do odbioru 0 - anulowane
                                             //public virtual Status Status { get; set; } = null!;

        [MaxLength(50)]
        [Required(ErrorMessage = "Miejscowość jest wymagana.")]
        public string Miejscowosc { get; set; } = string.Empty;

        [MaxLength(40)]
        [Required(ErrorMessage = "Ulica jest wymagana.")]
        public string Ulica { get; set; } = string.Empty;

        [MaxLength(10)]
        [Required(ErrorMessage = "Numer budynku jest wymagany.")]
        public string NumerBudynku { get; set; } = string.Empty;

        [MaxLength(10)]
        [Required(ErrorMessage = "Kod pocztowy jest wymagany.")]
        public string KodPocztowy { get; set; } = string.Empty;

        // Nowe pole określające typ dostawy: true - dostawa, false - odbiór osobisty
        public bool SposobDostawy { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [MaxLength(50)]
        public string Imie { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [MaxLength(50)]
        public string Nazwisko { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format adresu email.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Niepoprawny format numeru telefonu.")]
        public string NumerTelefonu { get; set; } = string.Empty;
        //public int DostawaId { get; set; }
        //public virtual Dostawa Dostawa { get; set; } = null!;
        public int? OpiniaId { get; set; }
        public virtual Opinia Opinia { get; set; }
        public string? UzytkownikId { get; set; }
        public virtual Uzytkownik Uzytkownik { get; set; } = null!;
        public virtual ICollection<ZamowienieProdukt> ZamowioneProdukty { get; set; } = new List<ZamowienieProdukt>();
        //public virtual Koszyk Koszyk { get; set; }
        //public int KoszykId { get; set; }
    }
}
