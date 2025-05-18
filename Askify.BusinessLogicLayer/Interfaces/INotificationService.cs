using Askify.BusinessLogicLayer.DTO;

namespace Askify.BusinessLogicLayer.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto?> GetByIdAsync(int id);
        Task<IEnumerable<NotificationDto>> GetForUserAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<int> CreateNotificationAsync(string userId, string type, int entityId, string message);
    }
}
