using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Post>> GetPostsWithAuthorAsync()
        {
            return await Task.FromResult(new List<Post>());
        }

        public async Task<Post?> GetPostWithCommentsAsync(Guid postId)
        {
            return await Task.FromResult<Post?>(null);
        }

        public async Task<IEnumerable<Post>> GetByUserIdAsync(string userId)
        {
            return await Task.FromResult(new List<Post>());
        }
    }

}
