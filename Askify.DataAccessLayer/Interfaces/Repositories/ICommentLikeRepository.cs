using Askify.DataAccessLayer.Entities;

namespace Askify.DataAccessLayer.Interfaces.Repositories
{
    public interface ICommentLikeRepository : IGenericRepository<CommentLike>
    {
        Task<int> GetLikeCountForCommentAsync(Guid commentId);
    }

}
