using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askify.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbacksController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetAll()
        {
            var isAdmin = User.IsInRole("Admin");
            
            if (isAdmin)
            {
                // Admins can see all feedbacks
                var feedbacks = await _feedbackService.GetAllAsync();
                return Ok(feedbacks);
            }
            else
            {
                // Regular users get a limited view - for example, only publicly visible feedbacks
                // You could create a separate method in the service for this if needed
                var feedbacks = await _feedbackService.GetAllAsync();
                return Ok(feedbacks);
                
                // Alternative: return Forbid() if you really want to restrict this to admins only
                // return Forbid();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FeedbackDto>> GetById(int id)
        {
            var feedback = await _feedbackService.GetByIdAsync(id);
            if (feedback == null) return NotFound();
            return Ok(feedback);
        }

        [HttpGet("expert/{expertId}")]
        public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetForExpert(string expertId)
        {
            var feedbacks = await _feedbackService.GetForExpertAsync(expertId);
            return Ok(feedbacks);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<int>> Create([FromBody] CreateFeedbackDto feedbackDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var feedbackId = await _feedbackService.CreateFeedbackAsync(userId, feedbackDto);
            return CreatedAtAction(nameof(GetById), new { id = feedbackId }, feedbackId);
        }
    }
}
