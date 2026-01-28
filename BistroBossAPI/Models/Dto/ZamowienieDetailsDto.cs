namespace BistroBossAPI.Models.Dto
{
    public class ZamowienieDetailsDto
    {
        public int Id { get; set; }
        public string? UzytkownikId { get; set; }
        public DateTime DataZamowienia { get; set; }
        public float CenaCalkowita { get; set; }
        public int PrzewidywanyCzasRealizacji { get; set; }
        public int Status { get; set; }
        public bool SposobDostawy { get; set; }

        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public string Email { get; set; }
        public string NumerTelefonu { get; set; }

        public string Miejscowosc { get; set; }
        public string Ulica { get; set; }
        public string NumerBudynku { get; set; }
        public string KodPocztowy { get; set; }

        public List<ZamowienieProduktDto> ZamowienieProdukty { get; set; }
        public OpiniaDto? Opinia { get; set; }
    }
}
