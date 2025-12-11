using System.ComponentModel.DataAnnotations;

namespace BistroBossAPI.Models
{
    public class Produkt
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(60)]
        public string Nazwa { get; set; } = string.Empty;
        [MaxLength(150)]
        public string Opis { get; set; } = string.Empty;
        public float Cena { get; set; }
        public int CzasPrzygotowania { get; set; }
        public string? Zdjecie { get; set; }
        public int KategoriaId { get; set; }
        public virtual Kategoria Kategoria { get; set; } = null!;
        public virtual ICollection<ZamowienieProdukt> ZamowieniaProduktu { get; set; } = new List<ZamowienieProdukt>();
        //public int KoszykId { get; set; }
        //public virtual ICollection<Koszyk> Koszyki { get; set; } = null!;
    }
}
