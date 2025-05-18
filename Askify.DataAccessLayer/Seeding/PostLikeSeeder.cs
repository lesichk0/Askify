using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class PostLikeSeeder
    {
        private readonly AppDbContext _context;

        public PostLikeSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.PostLikes.AnyAsync()) return;

            var faker = new Faker("uk");
            var users = await _context.Users.ToListAsync();
            var posts = await _context.Posts.Include(p => p.Author).ToListAsync();

            var likes = new List<PostLike>();

            foreach (var post in posts.Take(6))
            {
                var liker = faker.PickRandom(users.Where(u => u.Id != post.AuthorId).ToList());

                likes.Add(new PostLike
                {
                    PostId = post.Id,
                    UserId = liker.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-faker.Random.Int(1, 100))
                });
            }

            await _context.PostLikes.AddRangeAsync(likes);
            await _context.SaveChangesAsync();
        }
    }

}
