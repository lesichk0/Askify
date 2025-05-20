using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Askify.WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationsController : ControllerBase
    {
        private readonly IConsultationService _consultationService;
        private readonly ILogger<ConsultationsController> _logger;

        public ConsultationsController(IConsultationService consultationService, ILogger<ConsultationsController> logger)
        {
            _consultationService = consultationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetAll()
        {
            try
            {
                // Simple authentication check - any authenticated user can get all consultations
                var consultations = await _consultationService.GetAllAsync();
                return Ok(consultations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all consultations");
                return StatusCode(500, new { message = "An error occurred while retrieving consultations" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching consultation with ID: {Id}", id);
                
                var consultation = await _consultationService.GetByIdAsync(id);
                
                if (consultation == null)
                {
                    _logger.LogWarning("Consultation with ID {Id} not found", id);
                    return NotFound($"Consultation with ID {id} not found");
                }

                // Check if this is a public request (unauthenticated) or private request
                if (User.Identity == null || !User.Identity.IsAuthenticated)
                {
                    // For anonymous users, only return completed and public consultations
                    if (consultation.Status?.ToLower() != "completed" || !consultation.IsPublicable)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    // For authenticated users, check if they have access to this consultation
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    
                    // Users can see their own consultations, or ones where they're the expert
                    bool hasAccess = consultation.UserId == userId || consultation.ExpertId == userId;
                    
                    if (!hasAccess)
                    {
                        // If it's not their consultation, they can only see public, completed ones
                        if (consultation.Status?.ToLower() != "completed" || !consultation.IsPublicable)
                        {
                            return Forbid();
                        }
                    }
                }
                
                _logger.LogInformation("Successfully retrieved consultation with ID: {Id}", id);
                return Ok(consultation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultation with ID {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the consultation", detail = ex.Message });
            }
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetMyConsultations()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) 
                    return Unauthorized();

                // Get all consultations related to this user (as either owner or expert)
                var consultations = await _consultationService.GetConsultationsForUserAsync(userId);
                return Ok(consultations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user consultations");
                return StatusCode(500, new { message = "An error occurred while retrieving consultations" });
            }
        }

        [HttpGet("my/owner")]
        public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetMyOwnedConsultations()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) 
                    return Unauthorized();

                // Get only consultations where user is the owner
                var consultations = await _consultationService.GetConsultationsForUserAsync(userId, includeAllRoles: false);
                return Ok(consultations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user's owned consultations");
                return StatusCode(500, new { message = "An error occurred while retrieving consultations" });
            }
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
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && string.IsNullOrEmpty(userId) || consultation.UserId != userId)
                return Forbid();
                
            var result = await _consultationService.DeleteConsultationAsync(id);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var consultation = await _consultationService.GetByIdAsync(id);
            if (consultation == null) return NotFound();
            
            var expertId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(expertId) || consultation.ExpertId != expertId)
                return Forbid();
                
            var result = await _consultationService.CompleteConsultationAsync(id);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpPost("{id}/accept")]
        public async Task<IActionResult> Accept(int id)
        {
            var consultation = await _consultationService.GetByIdAsync(id);
            if (consultation == null) return NotFound();
            
            var expertId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(expertId))
                return Unauthorized();
                
            var result = await _consultationService.AcceptConsultationAsync(id, expertId);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var consultation = await _consultationService.GetByIdAsync(id);
            if (consultation == null) return NotFound();
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && string.IsNullOrEmpty(userId) || 
                (consultation.UserId != userId && consultation.ExpertId != userId))
                return Forbid();
                
            var result = await _consultationService.CancelConsultationAsync(id);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicConsultations()
        {
            try
            {
                // Force connection pool reset
                Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
                
                var unitOfWork = HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
                var consultations = await unitOfWork.Consultations
                    .FindAsync(c => c.Status == "Completed" && c.IsPublicable);
                
                return Ok(consultations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public consultations");
                return StatusCode(500, new { message = "An error occurred while retrieving consultations", detail = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetByUserId(string userId)
        {
            try
            {
                _logger.LogInformation("Fetching consultations for user ID: {UserId}", userId);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID is required");
                }
                
                // Get all consultations related to this user (either as owner or expert)
                var consultations = await _consultationService.GetConsultationsByUserIdAsync(userId);
                
                _logger.LogInformation("Found {Count} consultations for user ID: {UserId}", 
                    consultations.Count(), userId);
                    
                return Ok(consultations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultations for user with ID {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving consultations", detail = ex.Message });
            }
        }
    }
}
