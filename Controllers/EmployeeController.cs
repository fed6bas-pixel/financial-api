using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Malia.Data;
using Malia.Models;

namespace YourApi.Controllers
{
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        // ================= إنجاز المعاملة =================
        [HttpPut("complete/{transactionNumber}")]
        public async Task<IActionResult> Complete(string transactionNumber)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(x => x.TransactionNumber == transactionNumber);

            if (booking == null)
                return NotFound();

            // ✔ تغيير الحالة إلى Approved
            booking.Status = BookingStatus.Approved;

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إنجاز المعاملة" });
        }

        // ================= حجوزات اليوم =================
        [HttpGet("today-bookings")]
        public async Task<IActionResult> GetToday()
        {
            var today = DateTime.UtcNow.Date; //BookingDate = bookingDate.ToDateTime(TimeOnly.MinValue),

            var data = await _context.Bookings
                .Where(b => b.BookingDate.Date == today)
                .Select(b => new
                {
                   // b.Id,
                    b.SellerName,
                    b.BuyerName,
                    b.PropertyNumber,
                    b.TransactionNumber,
                    Status = (int)b.Status, 
                    b.BookingDate,
                    Source = "active"
                })
                .ToListAsync();

            return Ok(data);
        }

        // ================= البحث عن معاملة =================
        [HttpGet("search/{transactionNumber}")]
        public async Task<IActionResult> SearchTransaction(string transactionNumber)
        {
            // 🔵 1. البحث في الحجوزات الأساسية
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(x => x.TransactionNumber == transactionNumber);

            if (booking != null)
            {
                return Ok(new
                {
                    booking.TransactionNumber,
                    booking.SellerName,
                    booking.BuyerName,
                    Source = "active",
                    booking.PropertyNumber,
                    booking.BookingDate, 
                    Status = (int)booking.Status  , // ✔ رقم مو string /*  *//
                               
                  //  Source = "active"
                });
            }

            // 🔴 2. البحث في المحذوفات
            var deleted = await _context.DeletedBookings
                .FirstOrDefaultAsync(x => x.TransactionNumber == transactionNumber);

            if (deleted != null)
            {
                return Ok(new
                {
                    deleted.TransactionNumber,
                    deleted.SellerName,
                    deleted.BuyerName,
                    Source = "deleted",
                    Status = 2 // ✔ محذوفة ثابتة
                });
            }

            return NotFound(new { message = "لم يتم العثور على المعاملة" });
        }

        // ================= حذف (أرشفة) =================
        [HttpPost("delete/{transactionNumber}")]
        public async Task<IActionResult> Delete(string transactionNumber)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(x => x.TransactionNumber == transactionNumber);

            if (booking == null)
                return NotFound();

            // ✔ نقل إلى جدول المحذوفات
            var deleted = new DeletedBookings
            {
                UserId = booking.UserId,
                SellerName = booking.SellerName,
                BuyerName = booking.BuyerName,
                PropertyNumber = booking.PropertyNumber,
                BookingDate = booking.BookingDate,
                TransactionNumber = booking.TransactionNumber,
                DeletedAt = DateTime.Now
            };

            _context.DeletedBookings.Add(deleted);

            // _context.Bookings.Remove(booking);2 تعديل غير معروف 
            booking.IsDeleted = true;
            booking.Status = BookingStatus.Rejected;

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف المعاملة" });
        }
    }
}