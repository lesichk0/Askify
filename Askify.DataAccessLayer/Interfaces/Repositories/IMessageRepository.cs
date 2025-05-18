using Askify.DataAccessLayer.Entities;

namespace Askify.DataAccessLayer.Interfaces.Repositories
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<IEnumerable<Message>> GetMessagesForConsultationAsync(Guid consultationId);
        Task<IEnumerable<Message>> GetUserMessagesAsync(string userId);
    }
}
