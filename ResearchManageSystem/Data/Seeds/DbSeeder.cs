using ResearchManageSystem.Data.Entities;
using ResearchManageSystem.Enums;

namespace ResearchManageSystem.Data.Seeds
{
    public static class DbSeeder
    {
        public static async Task SeedData(ApplicationDbContext context)
        {
            try
            {
                if (!context.Faculties.Any())
            {
                    // Seed Faculties
                    var engineering = new Faculty
                    {
                        Name = "Faculty of Engineering",
                        Description = "Engineering Faculty",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    context.Faculties.Add(engineering);
                    await context.SaveChangesAsync();

                    // Seed Departments
                    var computerEng = new Department
                    {
                        Name = "Computer Engineering",
                        Description = "Computer Engineering Department",
                        FacultyId = engineering.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    context.Departments.Add(computerEng);
                    await context.SaveChangesAsync();

                    // Seed Users
                    var users = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        // รหัสผ่าน: Admin@123
                        Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        FirstName = "System",
                        LastName = "Admin",
                        Email = "admin@example.com",
                        Role = UserRole.Admin,
                        DepartmentId = computerEng.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new User
                    {
                        Username = "advisor",
                        // รหัสผ่าน: Advisor@123
                        Password = BCrypt.Net.BCrypt.HashPassword("Advisor@123"),
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "advisor@example.com",
                        Role = UserRole.Advisor,
                        DepartmentId = computerEng.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new User
                    {
                        Username = "student",
                        // รหัสผ่าน: Student@123
                        Password = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                        FirstName = "Jane",
                        LastName = "Smith",
                        Email = "student@example.com",
                        Role = UserRole.Student,
                        DepartmentId = computerEng.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                };

                    context.Users.AddRange(users);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}