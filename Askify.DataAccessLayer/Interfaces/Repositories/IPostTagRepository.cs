using Askify.DataAccessLayer.Entities;

namespace Askify.DataAccessLayer.Interfaces.Repositories
{
    public interface IPostTagRepository : IGenericRepository<PostTag>
    {
        Task<IEnumerable<Tag>> GetTagsForPostAsync(Guid postId);
    }

}
