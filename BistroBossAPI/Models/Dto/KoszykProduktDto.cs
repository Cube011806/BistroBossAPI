namespace BistroBossAPI.Models.Dto
{
    public class KoszykProduktDto
    {
        public int Id { get; set; }
        public int ProduktId { get; set; }
        public string Nazwa { get; set; } = string.Empty; 
        public float Cena { get; set; }
        public int Ilosc { get; set; }
        public string? Zdjecie { get; set; }
    }
}
