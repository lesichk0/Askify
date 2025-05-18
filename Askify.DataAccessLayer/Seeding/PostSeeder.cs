using Askify.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class PostSeeder
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public PostSeeder(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            // Skip if already seeded
            if (await _context.Posts.AnyAsync())
            {
                return;
            }

            await SeedPostsAsync();
        }

        private async Task SeedPostsAsync()
        {
            // Check if users exist before creating posts
            var users = await _userManager.Users.ToListAsync();
            if (!users.Any())
            {
                return; // Skip seeding if no users exist
            }

            var random = new Random();

            for (int i = 0; i < 20; i++)
            {
                // Make sure lists aren't empty before using Random
                if (users.Count == 0) break;

                var user = users[random.Next(users.Count)];

                var post = new Post
                {
                    Title = $"Sample Post {i + 1}",
                    Content = $"This is the content of sample post {i + 1}.",
                    AuthorId = user.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                };

                await _context.Posts.AddAsync(post);
            }

            await _context.SaveChangesAsync();
        }
    }
}
