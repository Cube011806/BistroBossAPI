namespace BistroBossAPI.Models.Dto
{
    public class UzytkownikListDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public int AccessLevel { get; set; }
    }
}
