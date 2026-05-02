using Microsoft.EntityFrameworkCore;
using Malia.Data;
using Malia.Models;

namespace Malia.Services
{
    public class BookingService
    {
        private readonly AppDbContext _context;

        // 🕘 إعدادات الدوام
        private readonly TimeSpan StartTime = new TimeSpan(9, 0, 0);
        private readonly TimeSpan EndTime = new TimeSpan(14, 0, 0);

        // ⏱️ مدة كل حجز (10 دقائق أو 15 حسب اختيارك)
        private readonly int SlotMinutes = 10;

        // 📊 الحد الأقصى للحجوزات يومياً
        private readonly int DailyLimit = 30;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        // ================= GET NEXT QUEUE NUMBER =================
        public async Task<int> GetNextQueueNumber(DateTime date)
        {
            var count = await _context.Bookings
                .Where(b => b.BookingDate.Date == date.Date)
                .CountAsync();

            return count + 1;
        }

        // ================= CALCULATE TIME SLOT =================
        public TimeSpan CalculateTimeSlot(int queueNumber)
        {
            var totalMinutes = (queueNumber - 1) * SlotMinutes;

            var slotTime = StartTime.Add(TimeSpan.FromMinutes(totalMinutes));

            // ⛔ إذا تجاوز الدوام
            if (slotTime >= EndTime)
            {
                throw new Exception("لا يوجد أوقات متاحة لهذا اليوم");
            }

            return slotTime;
        }

        // ================= CHECK DAILY CAPACITY =================
        public async Task<bool> IsDayFull(DateTime date)
        {
            var count = await _context.Bookings
                .CountAsync(b => b.BookingDate.Date == date.Date);

            return count >= DailyLimit;
        }

        // ================= CHECK SLOT CONFLICT =================
        public async Task<bool> IsSlotTaken(DateTime date, TimeSpan slot)
        {
            return await _context.Bookings
                .AnyAsync(b =>
                    b.BookingDate.Date == date.Date &&
                    b.TimeSlot == slot.ToString());
        }

        // ================= FULL VALIDATION =================
        public async Task ValidateBooking(DateTime date)
        {
            // 📊 check capacity
            if (await IsDayFull(date))
                throw new Exception("تم الوصول للحد الأقصى من الحجوزات لهذا اليوم");

            // 🕘 check working hours
            if (date.DayOfWeek == DayOfWeek.Friday ||
                date.DayOfWeek == DayOfWeek.Saturday)
                throw new Exception("الجمعة والسبت عطلة رسمية");
        }
    }
}