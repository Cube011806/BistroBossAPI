namespace BistroBossAPI.Models
{
    public class KoszykSessionProduktDto
    {
        public int ProduktId { get; set; }
        public string Nazwa { get; set; } = string.Empty;
        public int Ilosc { get; set; } = 1;
        public string Opis { get; set; } = string.Empty;
        public float Cena { get; set; }
        public int CzasPrzygotowania { get; set; }
        public string? Zdjecie { get; set; }
        public int KategoriaId { get; set; }
        public virtual Kategoria Kategoria { get; set; } = null!;
        public virtual ICollection<ZamowienieProdukt> ZamowieniaProduktu { get; set; } = new List<ZamowienieProdukt>();
    }
}