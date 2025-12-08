using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class FeedbackRepository : GenericRepository<Feedback>, IFeedbackRepository
    {
        private readonly AppDbContext _context;

        public FeedbackRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Feedback>> GetForExpertAsync(string expertId)
        {
            return await _context.Feedbacks
                .Where(f => f.ExpertId == expertId)
                .ToListAsync();
        }
    }

}
