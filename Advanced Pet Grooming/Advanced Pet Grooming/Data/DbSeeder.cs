using Advanced_Pet_Grooming.Models;

namespace Advanced_Pet_Grooming.Data
{
    public static class DbSeeder
    {
        public static void SeedAdmin(ApplicationDbContext context)
        {
            if (!context.Staffs.Any())
            {
                var admin = new Staff
                {
                    staff_name = "System Admin",
                    role = "Admin",
                    email = "admin@petgrooming.com",
                    phone_no = "0123456789",
                    password_hash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    account_status = "Active"
                };

                context.Staffs.Add(admin);
                context.SaveChanges();
            }
        }
    }
}