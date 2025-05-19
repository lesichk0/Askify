using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askify.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetUserSubscriptions(string userId)
        {
            var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(userId);
            return Ok(subscriptions);
        }

        [HttpGet("subscribers/{expertId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetSubscribers(string expertId)
        {
            var subscribers = await _subscriptionService.GetSubscribersAsync(expertId);
            return Ok(subscribers);
        }

        [HttpPost("{targetUserId}")]
        [Authorize]
        public async Task<IActionResult> Subscribe(string targetUserId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _subscriptionService.SubscribeAsync(userId, targetUserId);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpDelete("{targetUserId}")]
        [Authorize]
        public async Task<IActionResult> Unsubscribe(string targetUserId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _subscriptionService.UnsubscribeAsync(userId, targetUserId);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpGet("check/{targetUserId}")]
        [Authorize]
        public async Task<ActionResult<bool>> IsSubscribed(string targetUserId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var isSubscribed = await _subscriptionService.IsSubscribedAsync(userId, targetUserId);
            return Ok(isSubscribed);
        }
    }
}
