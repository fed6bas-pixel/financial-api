namespace Malia.Models
{
    public class DeletedBookings
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string SellerName { get; set; }
        public string BuyerName { get; set; }
        public string PropertyNumber { get; set; }
        public DateTime BookingDate { get; set; }
        public string TransactionNumber { get; set; }
        public DateTime DeletedAt { get; set; } = DateTime.Now;
    }
}
