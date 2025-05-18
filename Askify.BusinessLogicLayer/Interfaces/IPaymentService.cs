using Askify.BusinessLogicLayer.DTO;

namespace Askify.BusinessLogicLayer.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto?> GetByIdAsync(int id);
        Task<IEnumerable<PaymentDto>> GetPaymentsForUserAsync(string userId);
        Task<PaymentDto?> GetPaymentByConsultationIdAsync(int consultationId);
        Task<int> CreatePaymentAsync(string userId, int consultationId, decimal amount, string currency);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, string status);
    }
}
