namespace BistroBossAPI.Models.Dto
{
    public class KoszykDto
    {
        public int Id { get; set; }
        public string? UzytkownikId { get; set; }
        public List<KoszykProduktDto> KoszykProdukty { get; set; } = new List<KoszykProduktDto>();
    }
}
