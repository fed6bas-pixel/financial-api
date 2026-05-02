namespace Malia.Models
{
    public class BookingDay
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int MaxBookings { get; set; }

        public bool IsOpen { get; set; } = true;

        public List<Booking> Bookings { get; set; }
    }
}
