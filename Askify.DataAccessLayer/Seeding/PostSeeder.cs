using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class PostSeeder
    {
        private readonly AppDbContext _context;

        public PostSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Posts.AnyAsync()) return;

            var users = await _context.Users.ToListAsync();
            var consultations = await _context.Consultations.ToListAsync();
            var tags = await _context.Tags.ToListAsync();

            var faker = new Faker("uk");

            var posts = new List<Post>();

            for (int i = 0; i < 6; i++)
            {
                var author = faker.PickRandom(users);
                var relatedConsultation = faker.PickRandom(consultations);

                var post = new Post
                {
                    AuthorId = author.Id,
                    Title = faker.Lorem.Sentence(),
                    Content = faker.Lorem.Paragraphs(2),
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    RelatedConsultationId = relatedConsultation.Id,
                    CoverImageUrl = faker.Image.PicsumUrl()
                };

                posts.Add(post);
            }

            await _context.Posts.AddRangeAsync(posts);
            await _context.SaveChangesAsync();

            // Add tags to posts
            var postTags = new List<PostTag>();

            foreach (var post in posts)
            {
                var selectedTags = faker.PickRandom(tags, 2).ToList();
                foreach (var tag in selectedTags)
                {
                    postTags.Add(new PostTag
                    {
                        PostId = post.Id,
                        TagId = tag.Id
                    });
                }
            }

            await _context.PostTags.AddRangeAsync(postTags);
            await _context.SaveChangesAsync();
        }
    }
}
