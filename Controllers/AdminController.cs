using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Malia.Data;
using Malia.Models;
using Malia.Models.DTO;

namespace Malia.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _hasher;

        public AdminController(AppDbContext context, IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // =============== كل الحجوزات =================
        [HttpGet("all-bookings")]
        public async Task<IActionResult> GetAllBookings()
        {
            var data = await _context.Bookings
                   .OrderByDescending(b => b.BookingDate)
                    .ToListAsync();
            /*  var data = await _context.Bookings // 4 غير معروف 
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();    */

            return Ok(data);
        }

        // ================حجوزات اليوم =================
        [HttpGet("today-bookings")]
        public IActionResult GetTodayBookings()
        {
            var today = DateTime.Today;

            bool isHoliday = today.DayOfWeek == DayOfWeek.Friday
                          || today.DayOfWeek == DayOfWeek.Saturday;

            // 👉 نضيف حالة اليوم في Header
            Response.Headers["X-Is-Holiday"] = isHoliday.ToString();

            var bookings = _context.Bookings
                .Where(x => x.BookingDate.Date == today)
                .Select(x => new BookingDto
                {
                    
                      TransactionNumber = x.TransactionNumber,
                    BookingDate = x.BookingDate,
                    SellerName = x.SellerName,
                    BuyerName = x.BuyerName,
                    PropertyNumber = x.PropertyNumber ,
                       TimeSlot = x.TimeSlot,
                    //  Status = x.Status.ToString()
                    Status = x.Status == BookingStatus.Pending ? "قيد الانتظار" :
             x.Status == BookingStatus.Approved ? "مقبول" :
             x.Status == BookingStatus.Rejected ? "مرفوض" :
             "غير معروف"
                })
                .ToList();

            return Ok(bookings); // 👈 دائماً List مثل employees
        }
        // ================= عدد حجوزات اليوم =================
        [HttpGet("today-count")]
        public async Task<IActionResult> GetTodayCount()
        {
            var today = DateTime.Today;

            var count = await _context.Bookings
                .CountAsync(b => b.BookingDate.Date == today);

            return Ok(new { count });
        }

        // ================= 📊 عدد بين تاريخين =================
        [HttpGet("count-between")]
        public async Task<IActionResult> GetCountBetween(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest("Start date must be before end date");

            // نحدد بداية اليوم
            var start = startDate.Date;

            // نهاية اليوم (اليوم التالي بدون وقت)
            var end = endDate.Date.AddDays(1);

            var count = await _context.Bookings
                .CountAsync(b => b.BookingDate >= start && b.BookingDate < end);

            return Ok(new { count });
        }

        // =================عدد بتاريخ معين =================
        [HttpGet("count-by-date")]
        public async Task<IActionResult> CountByDate([FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out DateTime parsedDate))
                return BadRequest("Invalid date");

            var start = parsedDate.Date;
            var end = start.AddDays(1);

            var count = await _context.Bookings
                .CountAsync(b => b.BookingDate >= start && b.BookingDate < end);

            return Ok(new { count });
        }

        // ================= 👨‍💼 إنشاء موظف =================
        [HttpPost("create-employee")]
        public async Task<IActionResult> CreateEmployee(RegisterDto dto)
        {
            var exists = await _context.Users
                .AnyAsync(u => u.UserName == dto.Username);

            if (exists)
                return BadRequest("Username already exists");

            var user = new User
            {
                UserName = dto.Username,
                FullName = dto.FullName,
                Role = UserRole.Employee
            };

            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Employee created");
        }

        // ================= 👨‍💼 عرض الموظفين =================
        [HttpGet("all-employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var users = await _context.Users.Where(u => u.Role == UserRole.Employee) // _context.Employees
                .Where(x => !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    x.UserName,
                    x.FullName,
                    x.Role
                })
                .ToListAsync();

            return Ok(users);



        }

        // ================= 🗑 حذف موظف (Soft Delete) =================
        [HttpDelete("delete-employee/{username}")]
        public async Task<IActionResult> DeleteEmployee(string username)
        {
            var emp = await _context.Users.Where(u => u.Role == UserRole.Employee) //  _context.Employees
                .FirstOrDefaultAsync(x => x.UserName == username);

            if (emp == null)
                return NotFound();

            emp.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee deleted (soft delete)" });
        }

        // ================= 🔍 بحث عن معاملة =================
        [HttpGet("search-transaction/{transactionNumber}")]
        public async Task<IActionResult> SearchTransaction(string transactionNumber)
        {
            var booking = await _context.Bookings
                .Where(x => x.TransactionNumber == transactionNumber)
                .Select(x => new
                {
                    id = x.Id,
                    sellerName = x.SellerName,
                    buyerName = x.BuyerName,
                    propertyNumber = x.PropertyNumber,
                    transactionNumber = x.TransactionNumber,
                    status = x.Status,
                    bookingDate = x.BookingDate,
                    source = "active"
                })
                .FirstOrDefaultAsync();

            if (booking != null)
                return Ok(booking);

            var deleted = await _context.DeletedBookings
                .Where(x => x.TransactionNumber == transactionNumber)
                .Select(x => new
                {
                    id = x.Id,
                    sellerName = x.SellerName,
                    buyerName = x.BuyerName,
                    propertyNumber = x.PropertyNumber,
                    transactionNumber = x.TransactionNumber,
                    status = 3,
                    bookingDate = x.BookingDate,
                    deletedAt = x.DeletedAt,
                    source = "deleted"
                })
                .FirstOrDefaultAsync();

            if (deleted != null)
                return Ok(deleted);

            return NotFound();
        }

        // ================= 📦 المحذوفات =================
        [HttpGet("deleted-bookings")]
        public async Task<IActionResult> GetDeletedBookings() 
        {
            var data = await _context.DeletedBookings.ToListAsync();
            return Ok(data);
        }
    }
}