using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using AutoMapper;

namespace Askify.BusinessLogicLayer.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentDto?> GetByIdAsync(int id)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            return payment != null ? _mapper.Map<PaymentDto>(payment) : null;
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsForUserAsync(string userId)
        {
            var payments = await _unitOfWork.Payments.GetPaymentsForUserAsync(userId);
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<PaymentDto?> GetPaymentByConsultationIdAsync(int consultationId)
        {
            var payment = await _unitOfWork.Payments.GetPaymentByConsultationIdAsync(consultationId);
            return payment != null ? _mapper.Map<PaymentDto>(payment) : null;
        }

        public async Task<int> CreatePaymentAsync(string userId, int consultationId, decimal amount, string currency)
        {
            var payment = new Payment
            {
                UserId = userId,
                ConsultationId = consultationId,
                Amount = amount,
                Currency = currency,
                Status = "Pending",
                Provider = "Default",
                PaymentDate = DateTime.UtcNow
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.CompleteAsync();

            return payment.Id;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, string status)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null) return false;

            payment.Status = status;
            _unitOfWork.Payments.Update(payment);
            return await _unitOfWork.CompleteAsync();
        }
    }
}
