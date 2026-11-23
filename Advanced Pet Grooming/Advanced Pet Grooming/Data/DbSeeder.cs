
using Advanced_Pet_Grooming.Models;

namespace Advanced_Pet_Grooming.Data
{
    public static class DbSeeder
    {
        public static void SeedAdmin(ApplicationDbContext context)
        {
            var defaultStaffs = new List<Staff>
            {
                // Admin Account
                new Staff
                {
                    staff_name = "System Admin",
                    role = "Admin",
                    email = "admin@petgrooming.com",
                    phone_no = "0123456789",
                    password_hash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    account_status = "Active"
                },

                // Staff Account 01
                new Staff
                {
                    staff_name = "Staff 01",
                    role = "Staff",
                    email = "staff01@petgrooming.com",
                    phone_no = "0123456781",
                    password_hash = BCrypt.Net.BCrypt.HashPassword("Staff01@123"),
                    account_status = "Active"
                },

                // Staff Account 02
                new Staff
                {
                    staff_name = "Staff 02",
                    role = "Staff",
                    email = "staff02@petgrooming.com",
                    phone_no = "0123456782",
                    password_hash = BCrypt.Net.BCrypt.HashPassword("Staff02@123"),
                    account_status = "Active"
                },

                // Groomer Account 01
                new Staff
                {
                    staff_name = "Groomer 01",
                    role = "Groomer",
                    email = "groomer01@petgrooming.com",
                    phone_no = "0123456783",
                    password_hash = BCrypt.Net.BCrypt.HashPassword("Groomer01@123"),
                    account_status = "Active"
                },

                // Groomer Account 02
                new Staff
                {
                    staff_name = "Groomer 02",
                    role = "Groomer",
                    email = "groomer02@petgrooming.com",
                    phone_no = "0123456784",
                    password_hash = BCrypt.Net.BCrypt.HashPassword("Groomer02@123"),
                    account_status = "Active"
                }
            };

            foreach (var staff in defaultStaffs)
            {
                bool exists = context.Staffs.Any(
                    s => s.email == staff.email);

                if (!exists)
                {
                    context.Staffs.Add(staff);
                }
            }

            context.SaveChanges();
        }
    }
}
