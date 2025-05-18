using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class PostTagRepository : GenericRepository<PostTag>, IPostTagRepository
    {
        private readonly AppDbContext _context;

        public PostTagRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tag>> GetTagsForPostAsync(Guid postId)
        {
            return await Task.FromResult(new List<Tag>());
        }
    }

}
