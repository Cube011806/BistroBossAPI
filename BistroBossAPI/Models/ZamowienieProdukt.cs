using System.ComponentModel.DataAnnotations;

namespace BistroBossAPI.Models
{
    public class ZamowienieProdukt
    {
        [Key]
        public int Id { get; set; }
        public int ZamowienieId { get; set; }
        public virtual Zamowienie Zamowienie { get; set; } = null!;
        public int ProduktId { get; set; }
        public virtual Produkt Produkt { get; set; } = null!;
        public int Ilosc { get; set; } = 1;
        public float Cena { get; set; } = 0.0f;
    }
}
