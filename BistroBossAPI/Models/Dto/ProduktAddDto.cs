using System.ComponentModel.DataAnnotations;

namespace BistroBossAPI.Models.Dto
{
    public class ProduktAddDto
    {
        [MaxLength(60)]
        public string Nazwa { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string Opis { get; set; } = string.Empty;
        public float Cena { get; set; }
        public int CzasPrzygotowania { get; set; }
        public int KategoriaId { get; set; }
        public string? Zdjecie { get; set; }
    }
}
