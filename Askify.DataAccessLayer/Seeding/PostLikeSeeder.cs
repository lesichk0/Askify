using Askify.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class PostLikeSeeder
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public PostLikeSeeder(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            // Skip if already seeded
            if (await _context.PostLikes.AnyAsync())
            {
                return;
            }

            var posts = await _context.Posts.ToListAsync();
            var users = await _userManager.Users.ToListAsync();
            
            // Safety check
            if (!posts.Any() || !users.Any())
            {
                return;
            }
            
            var random = new Random();
            var createdLikes = new HashSet<string>(); // Track unique post-user combinations
            
            foreach (var post in posts)
            {
                // Determine how many likes this post gets (up to half of all users)
                int likesCount = users.Count > 0 ? random.Next(0, Math.Min(users.Count, 5)) : 0;
                
                // Skip if we can't create any likes
                if (likesCount == 0) continue;
                
                // Get a random subset of users for this post
                var userSubset = users.OrderBy(x => Guid.NewGuid()).Take(likesCount).ToList();
                
                foreach (var user in userSubset)
                {
                    // Ensure we don't create duplicate likes
                    string key = $"{post.Id}-{user.Id}";
                    if (createdLikes.Contains(key))
                    {
                        continue;
                    }
                    
                    var postLike = new PostLike
                    {
                        PostId = post.Id,
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
                    };
                    
                    _context.PostLikes.Add(postLike);
                    createdLikes.Add(key);
                }
            }
            
            await _context.SaveChangesAsync();
        }
    }

}
