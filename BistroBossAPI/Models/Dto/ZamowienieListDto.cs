namespace BistroBossAPI.Models.Dto
{
    public class ZamowienieListDto
    {
        public int Id { get; set; }
        public DateTime DataZamowienia { get; set; }
        public float CenaCalkowita { get; set; }
        public int Status { get; set; }
        public bool SposobDostawy { get; set; }
    }
}
