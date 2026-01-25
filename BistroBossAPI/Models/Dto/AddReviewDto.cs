namespace BistroBossAPI.Models.Dto
{
    public class AddReviewDto
    {
        public int ZamowienieId { get; set; }
        public byte Ocena { get; set; }
        public string Komentarz { get; set; }
    }
}
