using System.ComponentModel.DataAnnotations;

namespace BistroBossAPI.Models
{
    public class Kategoria
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Nazwa { get; set; } = string.Empty;
        public virtual ICollection<Produkt> Produkty { get; set; } = new List<Produkt>();
    }
}
