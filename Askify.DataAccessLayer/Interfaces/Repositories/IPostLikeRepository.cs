using Askify.DataAccessLayer.Entities;

namespace Askify.DataAccessLayer.Interfaces.Repositories
{
    public interface IPostLikeRepository : IGenericRepository<PostLike>
    {
        Task<int> GetLikeCountForPostAsync(Guid postId);
    }

}
