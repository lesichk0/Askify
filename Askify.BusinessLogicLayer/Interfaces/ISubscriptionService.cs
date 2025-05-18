using Askify.BusinessLogicLayer.DTO;

namespace Askify.BusinessLogicLayer.Interfaces
{
    public interface ISubscriptionService
    {
        Task<IEnumerable<SubscriptionDto>> GetUserSubscriptionsAsync(string userId);
        Task<IEnumerable<SubscriptionDto>> GetSubscribersAsync(string expertId);
        Task<bool> SubscribeAsync(string subscriberId, string targetUserId);
        Task<bool> UnsubscribeAsync(string subscriberId, string targetUserId);
        Task<bool> IsSubscribedAsync(string subscriberId, string targetUserId);
    }
}
