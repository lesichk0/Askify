using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using AutoMapper;

namespace Askify.BusinessLogicLayer.Services
{
    public class ConsultationService : IConsultationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ConsultationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ConsultationDto?> GetByIdAsync(int id)
        {
            var consultation = await _unitOfWork.Consultations.GetByIdAsync(id);
            return consultation != null ? _mapper.Map<ConsultationDto>(consultation) : null;
        }

        public async Task<IEnumerable<ConsultationDto>> GetAllAsync()
        {
            var consultations = await _unitOfWork.Consultations.GetAllAsync();
            return _mapper.Map<IEnumerable<ConsultationDto>>(consultations);
        }

        public async Task<IEnumerable<ConsultationDto>> GetByUserIdAsync(string userId)
        {
            var consultations = await _unitOfWork.Consultations.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ConsultationDto>>(consultations);
        }

        public async Task<IEnumerable<ConsultationDto>> GetByExpertIdAsync(string expertId)
        {
            var consultations = await _unitOfWork.Consultations.GetByExpertIdAsync(expertId);
            return _mapper.Map<IEnumerable<ConsultationDto>>(consultations);
        }

        public async Task<int> CreateConsultationAsync(string userId, CreateConsultationDto consultationDto)
        {
            var consultation = _mapper.Map<Consultation>(consultationDto);
            consultation.UserId = userId;
            consultation.IsFree = !await HasUsedFreeConsultationAsync(userId);
            consultation.Status = "Pending";
            consultation.CreatedAt = DateTime.UtcNow;
            
            // Set ExpertId to null when creating a new consultation
            // It will be set when an expert accepts the consultation
            consultation.ExpertId = null;
            
            await _unitOfWork.Consultations.AddAsync(consultation);
            await _unitOfWork.CompleteAsync();

            return consultation.Id;
        }

        public async Task<bool> UpdateConsultationAsync(int id, UpdateConsultationDto consultationDto)
        {
            var consultation = await _unitOfWork.Consultations.GetByIdAsync(id);
            if (consultation == null) return false;

            _mapper.Map(consultationDto, consultation);
            _unitOfWork.Consultations.Update(consultation);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteConsultationAsync(int id)
        {
            var consultation = await _unitOfWork.Consultations.GetByIdAsync(id);
            if (consultation == null) return false;

            _unitOfWork.Consultations.Remove(consultation);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> AcceptConsultationAsync(int id, string expertId)
        {
            var consultation = await _unitOfWork.Consultations.GetByIdAsync(id);
            if (consultation == null || consultation.Status != "Pending") return false;

            consultation.ExpertId = expertId;
            consultation.Status = "Accepted";
            _unitOfWork.Consultations.Update(consultation);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> CompleteConsultationAsync(int id)
        {
            var consultation = await _unitOfWork.Consultations.GetByIdAsync(id);
            if (consultation == null || consultation.Status != "Accepted") return false;

            consultation.Status = "Completed";
            _unitOfWork.Consultations.Update(consultation);

            // If this was a free consultation, mark the user as having used it
            if (consultation.IsFree)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(consultation.UserId);
                if (user != null)
                {
                    user.HasUsedFreeConsultation = true;
                    _unitOfWork.Users.Update(user);
                }
            }

            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> CancelConsultationAsync(int id)
        {
            var consultation = await _unitOfWork.Consultations.GetByIdAsync(id);
            if (consultation == null) return false;

            consultation.Status = "Cancelled";
            _unitOfWork.Consultations.Update(consultation);
            return await _unitOfWork.CompleteAsync();
        }

        private async Task<bool> HasUsedFreeConsultationAsync(string userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return user?.HasUsedFreeConsultation ?? false;
        }
    }
}
