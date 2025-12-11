using System.ComponentModel.DataAnnotations;

namespace BistroBossAPI.Models
{
    public class Dostawa
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Miejscowosc { get; set; } = string.Empty;
        [MaxLength(40)]
        public string Ulica { get; set; } = string.Empty;
        [MaxLength(10)]
        public string NumerBudynku { get; set; } = string.Empty;
        [MaxLength(10)]
        public string KodPocztowy { get; set; } = string.Empty;
        //public virtual ICollection<Zamowienie> Zamowienia { get; set; } = new List<Zamowienie>();
    }
}
