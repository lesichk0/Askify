using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class CommentSeeder
    {
        private readonly AppDbContext _context;

        public CommentSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Comments.AnyAsync()) return;

            var faker = new Faker("uk");
            var users = await _context.Users.ToListAsync();
            var posts = await _context.Posts.ToListAsync();

            var comments = new List<Comment>();

            foreach (var post in posts.Take(6))
            {
                var author = faker.PickRandom(users);
                comments.Add(new Comment
                {
                    PostId = post.Id,
                    AuthorId = author.Id,
                    Content = faker.Lorem.Sentences(2),
                    CreatedAt = DateTime.UtcNow.AddMinutes(-faker.Random.Int(1, 60))
                });
            }

            await _context.Comments.AddRangeAsync(comments);
            await _context.SaveChangesAsync();
        }
    }
}
