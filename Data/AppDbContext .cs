using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Malia.Models;

namespace Malia.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<BookingDay> BookingDays { get; set; }
        //public DbSet<Booking> Bookings { get; set; }
        public DbSet<Booking> Bookings { get; set; }
     //   public DbSet<Booking> Bookings { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<DeletedBookings> DeletedBookings { get; set; }
       // public DbSet<Employee> Employees { get; set; } 

    }
}