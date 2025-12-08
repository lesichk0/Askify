using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        public CommentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
        {
            return await _context.Set<Comment>()
                .Include(c => c.Author)
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
