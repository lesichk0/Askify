using Askify.BusinessLogicLayer.DTO;

namespace Askify.BusinessLogicLayer.Interfaces
{
    public interface IMessageService
    {
        Task<MessageDto?> GetByIdAsync(int id);
        Task<IEnumerable<MessageDto>> GetUserMessagesAsync(string userId);
        Task<IEnumerable<MessageDto>> GetMessagesForConsultationAsync(int consultationId);
        Task<int> SendMessageAsync(string senderId, CreateMessageDto messageDto);
        Task<bool> MarkAsReadAsync(int messageId);
    }
}
