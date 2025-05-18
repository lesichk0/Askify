using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class ConsultationRepository : GenericRepository<Consultation>, IConsultationRepository
    {
        private readonly AppDbContext _context;

        public ConsultationRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Consultation>> GetByExpertIdAsync(string expertId)
        {
            return await Task.FromResult(new List<Consultation>());
        }

        public async Task<IEnumerable<Consultation>> GetByUserIdAsync(string userId)
        {
            return await Task.FromResult(new List<Consultation>());
        }
    }

}
