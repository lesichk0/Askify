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
    public class ConsultationsController : ControllerBase
    {
        private readonly IConsultationService _consultationService;

        public ConsultationsController(IConsultationService consultationService)
        {
            _consultationService = consultationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            IEnumerable<ConsultationDto> consultations;
            
            if (isAdmin)
            {
                consultations = await _consultationService.GetAllAsync();
            }
            else
            {
                // Regular users can only see their own consultations
                consultations = await _consultationService.GetByUserIdAsync(userId);
            }
            
            return Ok(consultations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConsultationDto>> GetById(int id)
        {
            var consultation = await _consultationService.GetByIdAsync(id);
            if (consultation == null) return NotFound();
            
            // Check if the current user is part of this consultation
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && string.IsNullOrEmpty(userId) || 
                (consultation.UserId != userId && consultation.ExpertId != userId))
                return Forbid();
                
            return Ok(consultation);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetUserConsultations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var consultations = await _consultationService.GetByUserIdAsync(userId);
            return Ok(consultations);
        }

        [HttpGet("expert")]
        public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetExpertConsultations()
        {
            var expertId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(expertId)) return Unauthorized();

            var consultations = await _consultationService.GetByExpertIdAsync(expertId);
            return Ok(consultations);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateConsultationDto consultationDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var consultationId = await _consultationService.CreateConsultationAsync(userId, consultationDto);
            return CreatedAtAction(nameof(GetById), new { id = consultationId }, consultationId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateConsultationDto consultationDto)
        {
            var consultation = await _consultationService.GetByIdAsync(id);
            if (consultation == null) return NotFound();
            
            // Check if the current user is authorized to update
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && string.IsNullOrEmpty(userId) || 
                (consultation.UserId != userId && consultation.ExpertId != userId))
                return Forbid();
                
            var result = await _consultationService.UpdateConsultationAsync(id, consultationDto);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var consultation = await _consultationService.GetByIdAsync(id);
            if (consultation == null) return NotFound();
            
            // Only admin or the user who created it can delete it
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && string.IsNullOrEmpty(userId) || consultation.UserId != userId)
                return Forbid();
                
            var result = await _consultationService.DeleteConsultationAsync(id);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpPost("{id}/accept")]
        public async Task<IActionResult> Accept(int id)
        {
            // Only experts can accept consultations
            var expertId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(expertId)) return Unauthorized();

            var result = await _consultationService.AcceptConsultationAsync(id, expertId);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var consultation = await _consultationService.GetByIdAsync(id);
            if (consultation == null) return NotFound();
            
            // Only the assigned expert can mark as complete
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || consultation.ExpertId != userId)
                return Forbid();
                
            var result = await _consultationService.CompleteConsultationAsync(id);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var consultation = await _consultationService.GetByIdAsync(id);
            if (consultation == null) return NotFound();
            
            // User, expert, or admin can cancel
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && string.IsNullOrEmpty(userId) || 
                (consultation.UserId != userId && consultation.ExpertId != userId))
                return Forbid();
                
            var result = await _consultationService.CancelConsultationAsync(id);
            if (!result) return BadRequest();
            return Ok();
        }
    }
}
