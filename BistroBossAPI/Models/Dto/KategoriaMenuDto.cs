namespace BistroBossAPI.Models.Dto
{
    public class KategoriaMenuDto
    {
        public int Id { get; set; }
        public string Nazwa { get; set; } = string.Empty;
        public List<ProduktMenuDto> Produkty { get; set; } = new();
    }

}
