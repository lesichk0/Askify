using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Payment>> GetPaymentsForUserAsync(string userId)
        {
            return await Task.FromResult(new List<Payment>());
        }

        public async Task<Payment?> GetPaymentByConsultationIdAsync(int consultationId)
        {
            return await Task.FromResult<Payment?>(null);
        }
    }

}
