namespace Malia.Models.DTO
{
    public class BookingDto
    {
        public string TransactionNumber { get; set; }
        public DateTime BookingDate { get; set; }
        public string SellerName { get; set; }
        public string BuyerName { get; set; } 
    }
}
