using Askify.BusinessLogicLayer.DTO;

namespace Askify.BusinessLogicLayer.Interfaces
{
    public interface IConsultationService
    {
        Task<ConsultationDto?> GetByIdAsync(int id);
        Task<IEnumerable<ConsultationDto>> GetAllAsync();
        Task<IEnumerable<ConsultationDto>> GetByUserIdAsync(string userId);
        Task<IEnumerable<ConsultationDto>> GetByExpertIdAsync(string expertId);
        Task<int> CreateConsultationAsync(string userId, CreateConsultationDto consultationDto);
        Task<bool> UpdateConsultationAsync(int id, UpdateConsultationDto consultationDto);
        Task<bool> DeleteConsultationAsync(int id);
        Task<bool> AcceptConsultationAsync(int id, string expertId);
        Task<bool> CompleteConsultationAsync(int id);
        Task<bool> CancelConsultationAsync(int id);
    }
}
