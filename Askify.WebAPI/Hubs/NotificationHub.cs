using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Askify.WebAPI.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"User {userId} connected to NotificationHub. Connection ID: {Context.ConnectionId}");
            
            // Add user to their personal notification group
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"User {userId} disconnected from NotificationHub. Connection ID: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        // Send notification to specific user
        public async Task SendNotificationToUser(string userId, string message, string type)
        {
            _logger.LogInformation($"Sending {type} notification to {userId}: {message}");
            
            var notification = new
            {
                message,
                type,
                timestamp = DateTime.UtcNow
            };
            
            await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
        }

        // Broadcast notification to all connected users
        public async Task BroadcastNotification(string message, string type)
        {
            _logger.LogInformation($"Broadcasting {type} notification: {message}");
            
            var notification = new
            {
                message,
                type,
                timestamp = DateTime.UtcNow
            };
            
            await Clients.All.SendAsync("ReceiveNotification", notification);
        }
    }
}
