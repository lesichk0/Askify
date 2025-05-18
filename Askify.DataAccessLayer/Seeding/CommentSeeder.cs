using Askify.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class CommentSeeder
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public CommentSeeder(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            // Skip if already seeded
            if (await _context.Comments.AnyAsync())
            {
                return;
            }

            var posts = await _context.Posts.ToListAsync();
            var users = await _userManager.Users.ToListAsync();
            
            // Check if we have posts and users before proceeding
            if (!posts.Any() || !users.Any())
            {
                return; // Skip seeding if no posts or users exist
            }
            
            var random = new Random();
            
            foreach (var post in posts)
            {
                // Only create comments if we have users
                if (users.Count == 0) break;
                
                var commentCount = random.Next(0, 5);
                
                for (int i = 0; i < commentCount; i++)
                {
                    var user = users[random.Next(users.Count)];
                    
                    var comment = new Comment
                    {
                        Content = $"This is comment {i+1} on post {post.Title}",
                        AuthorId = user.Id,
                        PostId = post.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(-random.Next(1, 24))
                    };
                    
                    _context.Comments.Add(comment);
                }
            }
            
            await _context.SaveChangesAsync();
        }
    }
}
