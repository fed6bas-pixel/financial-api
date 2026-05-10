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
                .FirstOrDefaultAsync(x => x.TransactionNumber == transactionNumber && !x.IsDeleted);

            if (booking == null)
                return NotFound(new { message = "المعاملة غير موجودة أو محذوفة" });

            booking.Status = BookingStatus.Approved;

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إنجاز المعاملة" });
        }

        // ================= حجوزات اليوم =================
        [HttpGet("today-bookings")]
        public async Task<IActionResult> GetToday()
        {
            var today = DateTime.UtcNow.Date;

            var data = await _context.Bookings
                .Where(b => b.BookingDate.Date == today && !b.IsDeleted)
                .OrderBy(b => b.BookingDate)
                .Select(b => new
                {
                    b.TransactionNumber,
                    b.SellerName,
                    b.BuyerName,
                    b.PropertyNumber,
                    b.BookingDate,
                    Status = (int)b.Status,
                    Source = "active"
                })
                .ToListAsync();

            return Ok(data);
        }

        // ================= البحث عن معاملة =================
        [HttpGet("search/{transactionNumber}")]
        public async Task<IActionResult> SearchTransaction(string transactionNumber)
        {
            // 🔵 البحث في النشطة فقط
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(x => x.TransactionNumber == transactionNumber && !x.IsDeleted);

            if (booking != null)
            {
                return Ok(new
                {
                    booking.TransactionNumber,
                    booking.SellerName,
                    booking.BuyerName,
                    booking.PropertyNumber,
                    booking.BookingDate,
                    Status = (int)booking.Status,
                    Source = "active"
                });
            }

            // 🔴 البحث في الأرشيف
            var deleted = await _context.DeletedBookings
                .FirstOrDefaultAsync(x => x.TransactionNumber == transactionNumber);

            if (deleted != null)
            {
                return Ok(new
                {
                    deleted.TransactionNumber,
                    deleted.SellerName,
                    deleted.BuyerName,
                    deleted.PropertyNumber,
                    deleted.BookingDate,
                    Status = 2,
                    Source = "deleted"
                });
            }

            return NotFound(new { message = "لم يتم العثور على المعاملة" });
        }

        // ================= حذف (أرشفة) =================
        [HttpPost("delete/{transactionNumber}")]
        public async Task<IActionResult> Delete(string transactionNumber)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(x => x.TransactionNumber == transactionNumber && !x.IsDeleted);

            if (booking == null)
                return NotFound(new { message = "المعاملة غير موجودة أو محذوفة مسبقاً" });

            // نقل إلى الأرشيف
            var deleted = new DeletedBookings
            {
                UserId = booking.UserId,
                SellerName = booking.SellerName,
                BuyerName = booking.BuyerName,
                PropertyNumber = booking.PropertyNumber,
                BookingDate = booking.BookingDate,
                TransactionNumber = booking.TransactionNumber,
                DeletedAt = DateTime.UtcNow
            };

            _context.DeletedBookings.Add(deleted);


            // Soft Delete
            booking.IsDeleted = true;
            booking.Status = BookingStatus.Rejected;

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف المعاملة" });
        }
    }
}