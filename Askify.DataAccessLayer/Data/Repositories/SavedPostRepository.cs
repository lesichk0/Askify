using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class SavedPostRepository : GenericRepository<SavedPost>, ISavedPostRepository
    {
        private readonly AppDbContext _context;

        public SavedPostRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SavedPost>> GetSavedPostsForUserAsync(string userId)
        {
            return await Task.FromResult(new List<SavedPost>());
        }
    }

}
