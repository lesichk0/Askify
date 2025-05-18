using Askify.DataAccessLayer.Entities;

namespace Askify.DataAccessLayer.Interfaces.Repositories
{
    public interface ISubscriptionRepository : IGenericRepository<Subscription>
    {
        Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(string userId);
        Task<IEnumerable<Subscription>> GetSubscribersAsync(string expertId);
    }

}
