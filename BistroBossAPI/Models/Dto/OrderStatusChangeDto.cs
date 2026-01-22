namespace BistroBossAPI.Models.Dto
{
    public class OrderStatusChangeDto
    {
        public int OrderId { get; set; }
        public int NewStatus { get; set; }
    }
}
