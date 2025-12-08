using Askify.BusinessLogicLayer.DTO;

namespace Askify.BusinessLogicLayer.Interfaces
{
    public interface IFeedbackService
    {
        Task<FeedbackDto?> GetByIdAsync(int id);
        Task<IEnumerable<FeedbackDto>> GetAllAsync();
        Task<IEnumerable<FeedbackDto>> GetForExpertAsync(string expertId);
        Task<int> CreateFeedbackAsync(string userId, CreateFeedbackDto feedbackDto);
        Task<bool> HasUserRatedExpertAsync(string userId, string expertId);
        Task<bool> HasUserRatedConsultationAsync(string userId, int consultationId);
    }
}
