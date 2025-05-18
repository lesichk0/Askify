using Askify.DataAccessLayer.Entities;

namespace Askify.DataAccessLayer.Interfaces.Repositories
{
    public interface IFeedbackRepository : IGenericRepository<Feedback>
    {
        Task<IEnumerable<Feedback>> GetForExpertAsync(string expertId);
    }

}
