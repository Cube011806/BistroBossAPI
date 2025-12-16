namespace BistroBossAPI.Models.Dto
{
    public class ProduktMenuDto
    {
        public int Id { get; set; }
        public string Nazwa { get; set; } = string.Empty;
        public float Cena { get; set; }
        public int CzasPrzygotowania { get; set; }
        public string? Zdjecie { get; set; }
    }
}
