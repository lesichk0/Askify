using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class CommentLikeRepository : GenericRepository<CommentLike>, ICommentLikeRepository
    {
        private readonly AppDbContext _context;

        public CommentLikeRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> GetLikeCountForCommentAsync(Guid commentId)
        {
            return await Task.FromResult(0);
        }
    }

}
