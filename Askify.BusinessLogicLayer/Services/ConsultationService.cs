using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using AutoMapper;
using System.Threading.Tasks;

namespace Askify.BusinessLogicLayer.Services
{    public class ConsultationService : IConsultationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public ConsultationService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
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

        public async Task<IEnumerable<ConsultationDto>> GetConsultationsForUserAsync(string userId, bool includeAllRoles = true)
        {
            IEnumerable<Consultation> consultations;
            
            if (includeAllRoles)
            {
                // Get consultations where the user is either the owner or the expert
                consultations = await _unitOfWork.Consultations.FindAsync(c => 
                    c.UserId == userId || c.ExpertId == userId);
            }
            else
            {
                // Get consultations where the user is only the owner
                consultations = await _unitOfWork.Consultations.FindAsync(c => c.UserId == userId);
            }
            
            return _mapper.Map<IEnumerable<ConsultationDto>>(consultations);
        }

        // Implement methods required by interface
        public async Task<IEnumerable<ConsultationDto>> GetByUserIdAsync(string userId)
        {
            return await GetConsultationsForUserAsync(userId, includeAllRoles: false);
        }

        public async Task<IEnumerable<ConsultationDto>> GetByExpertIdAsync(string expertId)
        {
            var consultations = await _unitOfWork.Consultations.FindAsync(c => c.ExpertId == expertId);
            return _mapper.Map<IEnumerable<ConsultationDto>>(consultations);
        }

        public async Task<int> CreateConsultationAsync(string userId, CreateConsultationDto consultationDto)
        {
            var consultation = _mapper.Map<Consultation>(consultationDto);
            consultation.UserId = userId;
            consultation.IsFree = !await HasUsedFreeConsultationAsync(userId);
            consultation.Status = "Pending";
            consultation.CreatedAt = DateTime.UtcNow;
              // Only set ExpertId to null if it's an open request or if no ExpertId was provided
            // If ExpertId is provided, respect it and don't override it
            if (consultationDto.IsOpenRequest || string.IsNullOrEmpty(consultationDto.ExpertId))
            {
                consultation.ExpertId = null;
            }
              await _unitOfWork.Consultations.AddAsync(consultation);
            await _unitOfWork.CompleteAsync();

            // If an expert was specified, send them a notification about the new consultation request
            if (!string.IsNullOrEmpty(consultation.ExpertId))
            {
                // Get user's name for the notification
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                string userName = user?.FullName ?? "A user";
                
                await _notificationService.CreateNotificationAsync(
                    consultation.ExpertId,
                    "ConsultationRequest",
                    consultation.Id,
                    $"{userName} has requested a consultation with you");
            }

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
        }        public async Task<bool> AcceptConsultationAsync(int id, string expertId)
        {
            var consultation = await _unitOfWork.Consultations.GetByIdAsync(id);
            
            // Use case-insensitive comparison for status
            if (consultation == null || consultation.Status?.ToLower() != "pending") return false;

            consultation.ExpertId = expertId;
            consultation.Status = "Accepted"; // Keep status in proper case
            _unitOfWork.Consultations.Update(consultation);
            
            var result = await _unitOfWork.CompleteAsync();
            
            // Send notification to the user that their consultation was accepted
            if (result)
            {
                // Get expert's name for the notification
                var expert = await _unitOfWork.Users.GetByIdAsync(expertId);
                string expertName = expert?.FullName ?? "An expert";
                
                await _notificationService.CreateNotificationAsync(
                    consultation.UserId,
                    "ConsultationAccepted",
                    consultation.Id,
                    $"{expertName} has accepted your consultation request");
            }
            
            return result;
        }        public async Task<bool> CompleteConsultationAsync(int id)
        {
            var consultation = await _unitOfWork.Consultations.GetByIdAsync(id);
            if (consultation == null || consultation.Status?.ToLower() != "accepted") return false;

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
        }        public async Task<bool> CancelConsultationAsync(int id)
        {
            var consultation = await _unitOfWork.Consultations.GetByIdAsync(id);
            if (consultation == null) return false;
              // Store the previous status to check if this was a decline operation
            string previousStatus = consultation.Status;
            string? previousExpertId = consultation.ExpertId;

            consultation.Status = "Cancelled";
            _unitOfWork.Consultations.Update(consultation);
            var result = await _unitOfWork.CompleteAsync();
              // If result is successful and this was a decline operation (had an expert and was Pending)
            if (result && previousStatus?.ToLower() == "pending" && !string.IsNullOrEmpty(previousExpertId))
            {
                // Get expert's name for the notification
                var expert = await _unitOfWork.Users.GetByIdAsync(previousExpertId);
                string expertName = expert?.FullName ?? "The expert";
                
                // Notify the user that the expert declined
                await _notificationService.CreateNotificationAsync(
                    consultation.UserId,
                    "ConsultationDeclined",
                    consultation.Id,
                    $"{expertName} has declined your consultation request");
            }
            
            return result;
        }

        public async Task<int> GetUserConsultationsCountAsync(string userId)
        {
            var consultations = await _unitOfWork.Consultations.FindAsync(c => c.UserId == userId || c.ExpertId == userId);
            return consultations.Count();
        }

        private async Task<bool> HasUsedFreeConsultationAsync(string userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return user?.HasUsedFreeConsultation ?? false;
        }

        public async Task<IEnumerable<ConsultationDto>> GetConsultationsByUserIdAsync(string userId)
        {
            var consultations = await _unitOfWork.Consultations.FindAsync(
                c => c.UserId == userId || c.ExpertId == userId);
                
            return _mapper.Map<IEnumerable<ConsultationDto>>(consultations);
        }
    }
}
