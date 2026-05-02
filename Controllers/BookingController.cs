using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Malia.Data;
using Malia.Models.DTO;
using Malia.Models;
using Malia.Services;
using System.Security.Claims;

namespace Malia.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }
        /*  private readonly AppDbContext _context;
         private readonly BookingService _bookingService;

         public BookingController(AppDbContext context, BookingService bookingService)
         {
             _context = context;
             _bookingService = bookingService;
         } */
        // 9602297867

        // ================= MY BOOKINGS =================
        [Authorize(Roles = "Citizen")]
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId && !b.IsDeleted)
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new
                {
                    b.TransactionNumber,
                    b.BookingDate,
                    TimeSlot = b.TimeSlot , 
                    b.SellerName,
                    b.BuyerName,
                    b.PropertyNumber ,
                    status = (int)b.Status
                })
                .ToListAsync();

            return Ok(bookings);
        }
        /*   */
        private TimeSpan GenerateRandomTime()
        {
            var start = new TimeSpan(9, 0, 0);   // 9:00
            var end = new TimeSpan(13, 30, 0);   // 1:30

            var random = new Random();

            var totalMinutes = (int)(end - start).TotalMinutes;
            var randomMinutes = random.Next(totalMinutes);

            return start.Add(TimeSpan.FromMinutes(randomMinutes));
        }

        // ================= CREATE BOOKING =================
        [Authorize(Roles = "Citizen")]
        [HttpPost("book")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var bookingDate = dto.BookingDate;

            // منع الجمعة والسبت
            if (bookingDate.DayOfWeek == DayOfWeek.Friday ||
                bookingDate.DayOfWeek == DayOfWeek.Saturday)
            {
                return BadRequest("الجمعة والسبت عطلة رسمية");
            }

            // منع التاريخ القديم
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (bookingDate < today)
            {
                return BadRequest("لا يمكن الحجز في تاريخ قديم");
            }

            // منع الحجز المكرر بنفس اليوم
            var startOfDay = bookingDate.ToDateTime(TimeOnly.MinValue);
            var endOfDay = startOfDay.AddDays(1);

            var exists = await _context.Bookings.AnyAsync(b =>
                b.UserId == userId &&
                b.BookingDate >= startOfDay &&
                b.BookingDate < endOfDay
            );

            if (exists)
                return BadRequest("لديك حجز مسبق في هذا اليوم");

            // ================= CREATE BOOKING =================
            var booking = new Booking
            {
                UserId = userId,
                BookingDate = bookingDate.ToDateTime(TimeOnly.MinValue),
                SellerName = dto.SellerName,
                BuyerName = dto.BuyerName,
                PropertyNumber = dto.PropertyNumber,

                TransactionNumber = GenerateTicketNumber(),
                Status = BookingStatus.Pending,

                // 🔥 وقت عشوائي بين 9 و 1:30
                //  BookingTime = GenerateRandomTime().ToString(@"hh\:mm")
                TimeSlot = GenerateRandomTime().ToString(@"hh\:mm") 
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // ================= RESPONSE =================
            return Ok(new
            {
                success = true,
                transactionId = booking.TransactionNumber,
                sellerName = booking.SellerName,
                buyerName = booking.BuyerName,
                propertyNumber = booking.PropertyNumber,
                bookingDate = booking.BookingDate.ToString("yyyy-MM-dd"),
                // time = booking.BookingTime,
                time = booking.TimeSlot ,
                status = (int)booking.Status
            });
        }

        // ================= TICKET NUMBER =================
        private string GenerateTicketNumber()
        {
            return DateTime.UtcNow.Ticks.ToString()[5..15];
        }
    }
}
/*    [Authorize(Roles = "Citizen")]
          [HttpGet("my-bookings")]
          public async Task<IActionResult> GetMyBookings()
          {
              var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

              var bookings = await _context.Bookings
                  .Where(b => b.UserId == userId && !b.IsDeleted)
                  .OrderByDescending(b => b.BookingDate)
                  .ToListAsync();

              return Ok(bookings);
          }
          //   [HttpGet("my-bookings")] 3 غير معروف 
            public async Task<IActionResult> GetMyBookings()
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var bookings = await _context.Bookings
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.BookingDate)
                    .ToListAsync();

                return Ok(bookings);
            }   

        // ================= CREATE BOOKING =================
        [Authorize(Roles = "Citizen")]
        [HttpPost("book")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var bookingDate = dto.BookingDate;

            // 🔴 منع الحجز خارج أيام الدوام (مثال)
            if (bookingDate.DayOfWeek == DayOfWeek.Friday ||
                bookingDate.DayOfWeek == DayOfWeek.Saturday)
            {
                return BadRequest("الجمعة والسبت عطلة رسمية");
            }

            // 🔴 منع الحجز في المستقبل البعيد (اختياري)

            var today = DateOnly.FromDateTime(DateTime.Today);

            if (bookingDate < today)
            {
                return BadRequest("لا يمكن الحجز في تاريخ قديم");
            }



            var startOfDay = bookingDate.ToDateTime(TimeOnly.MinValue);
            var endOfDay = startOfDay.AddDays(1);

            var exists = await _context.Bookings.AnyAsync(b =>
                b.UserId == userId &&
                b.BookingDate >= startOfDay &&
                b.BookingDate < endOfDay
            );

            if (exists)
                return BadRequest("لديك حجز مسبق في هذا اليوم");

            var booking = new Booking
            {
                UserId = userId,
                //BookingDate = bookingDate,
                BookingDate = bookingDate.ToDateTime(TimeOnly.MinValue),
                SellerName = dto.SellerName,
                BuyerName = dto.BuyerName,
                PropertyNumber = dto.PropertyNumber,

                TransactionNumber = GenerateTicketNumber(),
                Status = BookingStatus.Pending
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // return Ok(booking);
            return Ok(new
            {
                success = true,
                transactionId = booking.TransactionNumber,
                sellerName = booking.SellerName,
                buyerName = booking.BuyerName,
                propertyNumber = booking.PropertyNumber,
                bookingDate = booking.BookingDate.ToString("yyyy-MM-dd"),
                // status = booking.Status.ToString(),
                status = (int)booking.Status, // عدلنا 

                // 🔥 الوقت (حالياً من السيرفر - بسيط)
                time = booking.BookingDate.ToString("HH:mm")
            });
            /*   return Ok(new
              {
                  success = true,
                  transactionId = booking.TransactionNumber,
                  time = booking.BookingDate.ToString("HH:mm")
              });  
        } */ 