using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class ReportRepository : GenericRepository<Report>, IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Report>> GetReportsByTargetIdAsync(string targetId)
        {
            return await Task.FromResult(new List<Report>());
        }
    }

}
