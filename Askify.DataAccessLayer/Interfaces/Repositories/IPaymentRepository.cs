using Askify.DataAccessLayer.Entities;

namespace Askify.DataAccessLayer.Interfaces.Repositories
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsForUserAsync(string userId);
        Task<Payment?> GetPaymentByConsultationIdAsync(Guid consultationId);
    }
}
