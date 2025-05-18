using Askify.DataAccessLayer.Entities;

namespace Askify.DataAccessLayer.Interfaces.Repositories
{
    public interface IConsultationRepository : IGenericRepository<Consultation>
    {
        Task<IEnumerable<Consultation>> GetByExpertIdAsync(string expertId);
        Task<IEnumerable<Consultation>> GetByUserIdAsync(string userId);
    }

}
