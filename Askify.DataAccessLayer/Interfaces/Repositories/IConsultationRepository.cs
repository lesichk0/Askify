using System.Linq.Expressions;
using Askify.DataAccessLayer.Entities;

namespace Askify.DataAccessLayer.Interfaces.Repositories
{
    public interface IConsultationRepository : IGenericRepository<Consultation>
    {
        Task<IEnumerable<Consultation>> GetByExpertIdAsync(string expertId);
        Task<IEnumerable<Consultation>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Consultation>> GetAllWithExpertAsync();
        Task<IEnumerable<Consultation>> FindWithExpertAsync(Expression<Func<Consultation, bool>> predicate);
    }

}
