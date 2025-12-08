using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<Message?> GetByIdAsync(object id)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == (int)id);
        }

        public async Task<IEnumerable<Message>> GetMessagesForConsultationAsync(int consultationId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ConsultationId == consultationId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetUserMessagesAsync(string userId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }
    }
}
