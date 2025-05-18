using Askify.DataAccessLayer.Entities;

namespace Askify.DataAccessLayer.Interfaces.Repositories
{
    public interface ISavedPostRepository : IGenericRepository<SavedPost>
    {
        Task<IEnumerable<SavedPost>> GetSavedPostsForUserAsync(string userId);
    }

}
