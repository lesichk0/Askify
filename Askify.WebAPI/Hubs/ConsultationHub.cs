using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Askify.WebAPI.Hubs
{
    [Authorize]
    public class ConsultationHub : Hub
    {
        private readonly ILogger<ConsultationHub> _logger;

        public ConsultationHub(ILogger<ConsultationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"User {userId} connected to ConsultationHub. Connection ID: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"User {userId} disconnected from ConsultationHub. Connection ID: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        // Join a consultation room
        public async Task JoinConsultation(int consultationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groupName = $"consultation_{consultationId}";
            
            _logger.LogInformation($"User {userId} joining consultation {consultationId} (Group: {groupName})");
            
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserJoinedConsultation", new { userId, consultationId });
        }

        // Leave a consultation room
        public async Task LeaveConsultation(int consultationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groupName = $"consultation_{consultationId}";
            
            _logger.LogInformation($"User {userId} leaving consultation {consultationId} (Group: {groupName})");
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeftConsultation", new { userId, consultationId });
        }

        // Send a message to a specific consultation
        public async Task SendConsultationMessage(int consultationId, string message, string senderName)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groupName = $"consultation_{consultationId}";
            
            _logger.LogInformation($"Message from {userId} in consultation {consultationId}: {message}");
            
            var messageData = new
            {
                consultationId,
                senderId = userId,
                senderName,
                text = message,
                sentAt = DateTime.UtcNow,
                status = "Sent"
            };
            
            await Clients.Group(groupName).SendAsync("ReceiveConsultationMessage", messageData);
        }

        // Accept consultation
        public async Task AcceptConsultation(int consultationId, string expertName)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groupName = $"consultation_{consultationId}";
            
            _logger.LogInformation($"Expert {userId} accepted consultation {consultationId}");
            
            var data = new
            {
                consultationId,
                expertId = userId,
                expertName,
                acceptedAt = DateTime.UtcNow
            };
            
            await Clients.Group(groupName).SendAsync("ConsultationAccepted", data);
        }

        // Decline consultation
        public async Task DeclineConsultation(int consultationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groupName = $"consultation_{consultationId}";
            
            _logger.LogInformation($"Expert {userId} declined consultation {consultationId}");
            
            var data = new
            {
                consultationId,
                expertId = userId,
                declinedAt = DateTime.UtcNow
            };
            
            await Clients.Group(groupName).SendAsync("ConsultationDeclined", data);
        }

        // Complete consultation
        public async Task CompleteConsultation(int consultationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groupName = $"consultation_{consultationId}";
            
            _logger.LogInformation($"Consultation {consultationId} marked as complete by {userId}");
            
            var data = new
            {
                consultationId,
                completedAt = DateTime.UtcNow
            };
            
            await Clients.Group(groupName).SendAsync("ConsultationCompleted", data);
        }
    }
}
