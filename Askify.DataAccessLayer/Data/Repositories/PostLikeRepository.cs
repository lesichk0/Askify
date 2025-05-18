using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class PostLikeRepository : GenericRepository<PostLike>, IPostLikeRepository
    {
        private readonly AppDbContext _context;

        public PostLikeRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> GetLikeCountForPostAsync(Guid postId)
        {
            return await Task.FromResult(0);
        }
    }

}
