using System.ComponentModel.DataAnnotations.Schema;

namespace BistroBossAPI.Models
{
    public class Koszyk
    {
        public int Id { get; set; }

        public string? UzytkownikId { get; set; } 
        public virtual Uzytkownik? Uzytkownik { get; set; }

        public virtual ICollection<KoszykProdukt> KoszykProdukty { get; set; } = new List<KoszykProdukt>();
        public int? ZamowienieId { get; set; }
        public virtual Zamowienie? Zamowienie { get; set; }

    }

}
