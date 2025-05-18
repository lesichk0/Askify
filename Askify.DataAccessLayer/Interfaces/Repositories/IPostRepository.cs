using Askify.DataAccessLayer.Entities;

namespace Askify.DataAccessLayer.Interfaces.Repositories
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<IEnumerable<Post>> GetPostsWithAuthorAsync();
        Task<Post?> GetPostWithCommentsAsync(Guid postId);
        Task<IEnumerable<Post>> GetByUserIdAsync(string userId);
    }
}
