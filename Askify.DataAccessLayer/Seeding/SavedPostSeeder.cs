using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class SavedPostSeeder
    {
        private readonly AppDbContext _context;

        public SavedPostSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.SavedPosts.AnyAsync()) return;

            var faker = new Faker("uk");
            var users = await _context.Users.ToListAsync();
            var posts = await _context.Posts.Include(p => p.Author).ToListAsync();

            var saves = new List<SavedPost>();

            foreach (var post in posts.Take(6))
            {
                var saver = faker.PickRandom(users.Where(u => u.Id != post.AuthorId).ToList());

                saves.Add(new SavedPost
                {
                    PostId = post.Id,
                    UserId = saver.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-faker.Random.Int(10, 100))
                });
            }

            await _context.SavedPosts.AddRangeAsync(saves);
            await _context.SaveChangesAsync();
        }
    }
}
