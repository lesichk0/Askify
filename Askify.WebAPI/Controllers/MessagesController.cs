using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askify.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetUserMessages()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var messages = await _messageService.GetUserMessagesAsync(userId);
            
            // Adding more information to the response to help debug seeding issues
            if (!messages.Any())
            {
                return Ok(new { 
                    Messages = messages,
                    Note = "No messages found. This could indicate that messages haven't been seeded yet."
                });
            }
            
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MessageDto>> GetById(int id)
        {
            var message = await _messageService.GetByIdAsync(id);
            if (message == null) return NotFound();
            
            // Make sure the user is either the sender or receiver
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || (message.SenderId != userId && message.ReceiverId != userId))
                return Forbid();
                
            return Ok(message);
        }

        [HttpGet("consultation/{consultationId}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetForConsultation(int consultationId)
        {
            // Note: You may want to add additional authorization to check if the user 
            // is part of the consultation, but that would require injecting IConsultationService
            var messages = await _messageService.GetMessagesForConsultationAsync(consultationId);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<ActionResult<int>> SendMessage([FromBody] CreateMessageDto messageDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var messageId = await _messageService.SendMessageAsync(userId, messageDto);
            return CreatedAtAction(nameof(GetById), new { id = messageId }, messageId);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _messageService.GetByIdAsync(id);
            if (message == null) return NotFound();
            
            // Only the receiver can mark a message as read
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || message.ReceiverId != userId)
                return Forbid();
                
            var result = await _messageService.MarkAsReadAsync(id);
            if (!result) return BadRequest();
            return Ok();
        }
    }
}
