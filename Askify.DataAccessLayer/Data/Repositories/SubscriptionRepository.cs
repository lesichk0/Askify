using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
    {
        private readonly AppDbContext _context;

        public SubscriptionRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(string userId)
        {
            return await Task.FromResult(new List<Subscription>());
        }

        public async Task<IEnumerable<Subscription>> GetSubscribersAsync(string expertId)
        {
            return await Task.FromResult(new List<Subscription>());
        }
    }

}
