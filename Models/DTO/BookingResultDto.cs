namespace Malia.Models.DTO
{
    public class BookingResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int QueueNumber { get; set; }
        public string TransactionNumber { get; set; }
        public string TimeSlot { get; set; }
    }
}
