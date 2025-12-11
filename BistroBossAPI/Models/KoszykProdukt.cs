namespace BistroBossAPI.Models
{
    public class KoszykProdukt
    {
        public int Id { get; set; }

        public int KoszykId { get; set; }
        public virtual Koszyk Koszyk { get; set; } = null!;

        public int ProduktId { get; set; }
        public virtual Produkt Produkt { get; set; } = null!;

        public int Ilosc { get; set; } = 1;
    }

}
