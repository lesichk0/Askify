using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsForUserAsync(string userId)
        {
            return await Task.FromResult(new List<Payment>());
        }

        public async Task<Payment?> GetPaymentByConsultationIdAsync(Guid consultationId)
        {
            return await Task.FromResult<Payment?>(null);
        }
    }

}
