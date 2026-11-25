using Advanced_Pet_Grooming.Models;
using Microsoft.EntityFrameworkCore;

namespace Advanced_Pet_Grooming.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent multiple cascade delete path conflicts in SQL Server
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany() //c => c.Appointments
                .HasForeignKey(a => a.customer_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Pet)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.pet_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Staff)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.staff_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Staff)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.staff_id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}