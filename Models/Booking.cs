using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Malia.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string SellerName { get; set; }
        public string BuyerName { get; set; }
        public string PropertyNumber { get; set; }

        public DateTime BookingDate { get; set; }

        public int? QueueNumber { get; set; }
        public string? TimeSlot { get; set; }

        public string TransactionNumber { get; set; }
       
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public bool IsDeleted { get; set; } = false; 

    }
}