using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using AutoMapper;

namespace Askify.BusinessLogicLayer.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<NotificationDto?> GetByIdAsync(int id)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            return notification != null ? _mapper.Map<NotificationDto>(notification) : null;
        }

        public async Task<IEnumerable<NotificationDto>> GetForUserAsync(string userId)
        {
            var notifications = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId);
            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            _unitOfWork.Notifications.Update(notification);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var notifications = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead);
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                _unitOfWork.Notifications.Update(notification);
            }
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<int> CreateNotificationAsync(string userId, string type, int entityId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                EntityId = entityId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            return notification.Id;
        }

        public async Task<bool> DeleteAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null) return false;

            _unitOfWork.Notifications.Remove(notification);
            return await _unitOfWork.CompleteAsync();
        }
    }
}
