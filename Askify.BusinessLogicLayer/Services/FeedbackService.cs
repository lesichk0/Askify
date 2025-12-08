using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using AutoMapper;

namespace Askify.BusinessLogicLayer.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<FeedbackDto?> GetByIdAsync(int id)
        {
            var feedback = await _unitOfWork.Feedbacks.GetByIdAsync(id);
            return feedback != null ? _mapper.Map<FeedbackDto>(feedback) : null;
        }

        public async Task<IEnumerable<FeedbackDto>> GetAllAsync()
        {
            var feedbacks = await _unitOfWork.Feedbacks.GetAllAsync();
            return _mapper.Map<IEnumerable<FeedbackDto>>(feedbacks);
        }

        public async Task<IEnumerable<FeedbackDto>> GetForExpertAsync(string expertId)
        {
            var feedbacks = await _unitOfWork.Feedbacks.GetForExpertAsync(expertId);
            return _mapper.Map<IEnumerable<FeedbackDto>>(feedbacks);
        }

        public async Task<int> CreateFeedbackAsync(string userId, CreateFeedbackDto feedbackDto)
        {
            if (!feedbackDto.ConsultationId.HasValue)
            {
                throw new InvalidOperationException("ConsultationId is required.");
            }
            
            // Get the specific consultation
            var consultation = await _unitOfWork.Consultations.GetByIdAsync(feedbackDto.ConsultationId.Value);
            
            if (consultation == null)
            {
                throw new InvalidOperationException("Consultation not found.");
            }
            
            // Verify the user is the owner of this consultation
            if (consultation.UserId != userId)
            {
                throw new InvalidOperationException("You can only rate consultations you created.");
            }
            
            // Verify the consultation is completed
            if (consultation.Status == null || !consultation.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("You can only rate an expert after completing a consultation with them.");
            }
            
            // Verify the expert matches
            if (consultation.ExpertId != feedbackDto.ExpertId)
            {
                throw new InvalidOperationException("Expert ID does not match the consultation.");
            }
            
            // Check if user has already rated THIS consultation
            var existingFeedback = await _unitOfWork.Feedbacks.FindAsync(
                f => f.UserId == userId && f.ConsultationId == feedbackDto.ConsultationId.Value);
            
            if (existingFeedback.Any())
            {
                throw new InvalidOperationException("You have already rated this consultation.");
            }
            
            var feedback = _mapper.Map<Feedback>(feedbackDto);
            feedback.UserId = userId;
            feedback.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Feedbacks.AddAsync(feedback);
            await _unitOfWork.CompleteAsync();

            return feedback.Id;
        }

        public async Task<bool> HasUserRatedExpertAsync(string userId, string visibleId)
        {
            // visibleId can be either expertId (legacy) or consultationId
            // Try parsing as int for consultationId first
            if (int.TryParse(visibleId, out int consultationId))
            {
                var existingFeedback = await _unitOfWork.Feedbacks.FindAsync(
                    f => f.UserId == userId && f.ConsultationId == consultationId);
                return existingFeedback.Any();
            }
            
            // Fallback: treat as expertId (for backward compatibility)
            var feedbackByExpert = await _unitOfWork.Feedbacks.FindAsync(
                f => f.UserId == userId && f.ExpertId == visibleId);
            return feedbackByExpert.Any();
        }

        public async Task<bool> HasUserRatedConsultationAsync(string userId, int consultationId)
        {
            var existingFeedback = await _unitOfWork.Feedbacks.FindAsync(
                f => f.UserId == userId && f.ConsultationId == consultationId);
            return existingFeedback.Any();
        }
    }
}
