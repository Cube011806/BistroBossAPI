namespace BistroBossAPI.Models.Dto
{
    public class ReOrderRequestDto
    {
        public bool SposobDostawy { get; set; }
        public string Miejscowosc { get; set; }
        public string Ulica { get; set; }
        public string NumerBudynku { get; set; }
        public string KodPocztowy { get; set; }
    }
}

