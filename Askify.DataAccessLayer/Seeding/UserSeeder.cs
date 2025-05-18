using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class UserSeeder
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserSeeder(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            if (await _context.Users.AnyAsync()) return;

            var faker = new Faker("uk");

            var users = new List<User>();

            for (int i = 0; i < 6; i++)
            {
                var user = new User
                {
                    UserName = $"user{i}@mail.com",
                    Email = $"user{i}@mail.com",
                    FullName = faker.Name.FullName(),
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    EmailConfirmed = true
                };

                var password = "Test1234!"; // 👈 simple password for seeding
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    string role = i == 0 ? "Admin" : (i % 2 == 0 ? "Expert" : "User");
                    await _userManager.AddToRoleAsync(user, role);
                }

                users.Add(user);
            }
        }
    }
}
