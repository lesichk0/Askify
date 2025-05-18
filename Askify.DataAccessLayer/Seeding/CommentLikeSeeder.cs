using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class CommentLikeSeeder
    {
        private readonly AppDbContext _context;

        public CommentLikeSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.CommentLikes.AnyAsync()) return;

            var faker = new Faker("uk");
            var users = await _context.Users.ToListAsync();
            var comments = await _context.Comments.Include(c => c.Author).ToListAsync();

            var likes = new List<CommentLike>();

            foreach (var comment in comments.Take(6))
            {
                var liker = faker.PickRandom(users.Where(u => u.Id != comment.AuthorId).ToList());

                likes.Add(new CommentLike
                {
                    CommentId = comment.Id,
                    UserId = liker.Id,
                    CreatedAt = DateTime.UtcNow.AddSeconds(-faker.Random.Int(30, 300))
                });
            }

            await _context.CommentLikes.AddRangeAsync(likes);
            await _context.SaveChangesAsync();
        }
    }

}
