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
            var feedback = _mapper.Map<Feedback>(feedbackDto);
            feedback.UserId = userId;
            feedback.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Feedbacks.AddAsync(feedback);
            await _unitOfWork.CompleteAsync();

            return feedback.Id;
        }
    }
}
