using System.ComponentModel.DataAnnotations;

namespace BistroBossAPI.Models.Dto
{
    public class ZamowienieAddDto
    {
        public string UserId { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [MaxLength(50)]
        public string Imie { get; set; } = "";

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [MaxLength(50)]
        public string Nazwisko { get; set; } = "";

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format adresu email.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Niepoprawny format numeru telefonu.")]
        public string NumerTelefonu { get; set; } = "";

        [Required(ErrorMessage = "Miejscowość jest wymagana.")]
        [MaxLength(50)]
        public string Miejscowosc { get; set; } = "";

        [Required(ErrorMessage = "Ulica jest wymagana.")]
        [MaxLength(40)]
        public string Ulica { get; set; } = "";

        [Required(ErrorMessage = "Numer budynku jest wymagany.")]
        [MaxLength(10)]
        public string NumerBudynku { get; set; } = "";

        [Required(ErrorMessage = "Kod pocztowy jest wymagany.")]
        [MaxLength(10)]
        public string KodPocztowy { get; set; } = "";
        public bool SposobDostawy { get; set; } = true;

        public bool IsGuest { get; set; }
    }
}
