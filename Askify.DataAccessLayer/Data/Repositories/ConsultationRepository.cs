using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class ConsultationRepository : GenericRepository<Consultation>, IConsultationRepository
    {
        private readonly AppDbContext _context;

        public ConsultationRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<Consultation?> GetByIdAsync(object id)
        {
            if (id is not int consultationId) return null;
            return await _context.Consultations
                .Include(c => c.Expert)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == consultationId);
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
