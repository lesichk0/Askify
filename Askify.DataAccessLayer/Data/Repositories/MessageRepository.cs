using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Message>> GetMessagesForConsultationAsync(int consultationId)
        {
            return await Task.FromResult(new List<Message>());
        }

        public async Task<IEnumerable<Message>> GetUserMessagesAsync(string userId)
        {
            return await Task.FromResult(new List<Message>());
        }
    }
}
