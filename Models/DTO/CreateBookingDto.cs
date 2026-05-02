namespace Malia.Models.DTO
{

    public class CreateBookingDto
    {
        //  public DateOnly BookingDate { get; set; }
        public DateOnly BookingDate { get; set; }
        public string SellerName { get; set; }
        public string BuyerName { get; set; }
        public string PropertyNumber { get; set; }
    }
}