using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using AutoMapper;

namespace Askify.BusinessLogicLayer.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubscriptionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SubscriptionDto>> GetUserSubscriptionsAsync(string userId)
        {
            var subscriptions = await _unitOfWork.Subscriptions.GetUserSubscriptionsAsync(userId);
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetSubscribersAsync(string expertId)
        {
            var subscriptions = await _unitOfWork.Subscriptions.GetSubscribersAsync(expertId);
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<bool> SubscribeAsync(string subscriberId, string targetUserId)
        {
            if (subscriberId == targetUserId) return false;

            var existing = await _unitOfWork.Subscriptions.FindAsync(
                s => s.SubscriberId == subscriberId && s.TargetUserId == targetUserId);
            
            if (existing.Any()) return true; // Already subscribed

            var subscription = new Subscription
            {
                SubscriberId = subscriberId,
                TargetUserId = targetUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Subscriptions.AddAsync(subscription);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> UnsubscribeAsync(string subscriberId, string targetUserId)
        {
            var existing = await _unitOfWork.Subscriptions.FindAsync(
                s => s.SubscriberId == subscriberId && s.TargetUserId == targetUserId);
            
            if (!existing.Any()) return true; // Not subscribed

            _unitOfWork.Subscriptions.Remove(existing.First());
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> IsSubscribedAsync(string subscriberId, string targetUserId)
        {
            var existing = await _unitOfWork.Subscriptions.FindAsync(
                s => s.SubscriberId == subscriberId && s.TargetUserId == targetUserId);
            
            return existing.Any();
        }
    }
}
