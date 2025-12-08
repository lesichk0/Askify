using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Interfaces;
using Askify.WebAPI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<ConsultationHub> _hubContext;
        private readonly ICategoryClassificationService _classificationService;

        public ConsultationsController(
            IConsultationService consultationService, 
            ILogger<ConsultationsController> logger, 
            IHubContext<ConsultationHub> hubContext,
            ICategoryClassificationService classificationService)
        {
            _consultationService = consultationService;
            _logger = logger;
            _hubContext = hubContext;
            _classificationService = classificationService;
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<string>> GetCategories()
        {
            var categories = _classificationService.GetAllCategories();
            return Ok(categories);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetAll()
        {
            try
            {
                // Allow anonymous access - filter happens on frontend
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
                {                // For authenticated users, check if they have access to this consultation
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                    
                    // Users can see their own consultations, or ones where they're the expert
                    bool hasAccess = consultation.UserId == userId || consultation.ExpertId == userId;
                    
                    // Experts can see all pending consultations too (so they can accept them)
                    bool isExpert = userRole == "Expert";
                    
                    if (!hasAccess && !isExpert)
                    {
                        // If it's not their consultation and they're not an expert, they can only see public, completed ones
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
            
            // Broadcast real-time notification to the expert if specified
            if (!string.IsNullOrEmpty(consultationDto.ExpertId))
            {
                await _hubContext.Clients.User(consultationDto.ExpertId).SendAsync("NewConsultationRequest", new
                {
                    consultationId = consultationId,
                    userId = userId,
                    title = consultationDto.Title,
                    message = "You have a new consultation request"
                });
            }
            
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
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Allow both the client (UserId) and the expert (ExpertId) to complete the consultation
            if (string.IsNullOrEmpty(userId) || (consultation.ExpertId != userId && consultation.UserId != userId))
                return Forbid();
                
            var result = await _consultationService.CompleteConsultationAsync(id);
            if (!result) return BadRequest();
            
            // Notify via SignalR so both parties get the update
            await _hubContext.Clients.Group($"consultation_{id}").SendAsync("ConsultationCompleted", new 
            { 
                consultationId = id, 
                completedBy = userId,
                completedAt = DateTime.UtcNow 
            });
            
            return Ok();
        }        [HttpPost("{id}/accept")]
        public async Task<IActionResult> Accept(int id)
        {
            var consultation = await _consultationService.GetByIdAsync(id);
            if (consultation == null) return NotFound();
            
            // Get the expert ID from the authenticated user
            var expertId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            // Check authorization
            if (string.IsNullOrEmpty(expertId))
                return Unauthorized();
                
            // Make sure the user is actually an expert
            if (role != "Expert")
                return Forbid("Only experts can accept consultations");
                
            // Make sure the consultation is still in Pending status
            if (consultation.Status?.ToLower() != "pending")
                return BadRequest("This consultation is no longer available for acceptance");
                
            var result = await _consultationService.AcceptConsultationAsync(id, expertId);
            if (!result) return BadRequest();
            
            // Get updated consultation to get expert name
            var updatedConsultation = await _consultationService.GetByIdAsync(id);
            if (updatedConsultation != null)
            {
                var groupName = $"consultation_{id}";
                
                // Broadcast to all clients in the consultation group
                await _hubContext.Clients.Group(groupName).SendAsync("ConsultationAccepted", new
                {
                    consultationId = id,
                    expertId,
                    expertName = updatedConsultation.ExpertName,
                    acceptedAt = DateTime.UtcNow
                });
            }
            
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
            
            // Check cancel permissions based on consultation status and user role
            var isClient = consultation.UserId == userId;
            var isExpert = consultation.ExpertId == userId;
            var status = consultation.Status?.ToLower();
            
            // Expert can only cancel free consultations or pending consultations
            if (isExpert && !isAdmin)
            {
                if (status != "pending" && !consultation.IsFree)
                {
                    return BadRequest(new { message = "Expert can only cancel free consultations or decline pending requests." });
                }
            }
            
            // For accepted/inprogress paid consultations, only client can cancel
            if (!isClient && !isAdmin && (status == "accepted" || status == "inprogress") && !consultation.IsFree)
            {
                return BadRequest(new { message = "Only the client can cancel an ongoing paid consultation." });
            }
                
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

        [HttpGet("expert/available")]
        [Authorize(Roles = "Expert")]
        public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetAvailableForExperts()
        {
            try
            {
                _logger.LogInformation("Getting consultations available for experts");
                
                var expertId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(expertId))
                {
                    return Unauthorized();
                }
                
                // Get all consultations
                var allConsultations = await _consultationService.GetAllAsync();
                
                // Filter for open and pending consultations that don't already have this expert
                var availableConsultations = allConsultations.Where(c => 
                    c.Status?.ToLower() == "pending" && 
                    (string.IsNullOrEmpty(c.ExpertId) || c.ExpertId != expertId));
                
                return Ok(availableConsultations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultations available for experts");
                return StatusCode(500, new { message = "An error occurred while retrieving consultations" });
            }
        }

        [HttpPost("{id}/set-price")]
        [Authorize(Roles = "Expert")]
        public async Task<IActionResult> SetPrice(int id, [FromBody] SetPriceDto dto)
        {
            try
            {
                var expertId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(expertId))
                    return Unauthorized();

                if (dto.Price <= 0)
                    return BadRequest(new { message = "Price must be greater than 0" });

                var result = await _consultationService.SetPriceAsync(id, expertId, dto.Price);
                if (!result)
                    return BadRequest(new { message = "Failed to set price. Make sure the consultation is accepted and not free." });

                // Notify via SignalR
                await _hubContext.Clients.Group($"consultation_{id}").SendAsync("PriceUpdated", new { consultationId = id, price = dto.Price });

                return Ok(new { message = "Price set successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting price for consultation {Id}", id);
                return StatusCode(500, new { message = "An error occurred while setting the price" });
            }
        }

        [HttpPost("{id}/accept-price")]
        [Authorize]
        public async Task<IActionResult> AcceptPrice(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _consultationService.AcceptPriceAsync(id, userId);
                if (!result)
                    return BadRequest(new { message = "Failed to accept price. Make sure you are the consultation owner and the consultation is awaiting payment." });

                // Notify via SignalR
                await _hubContext.Clients.Group($"consultation_{id}").SendAsync("PaymentAccepted", new { consultationId = id });

                return Ok(new { message = "Payment accepted. Consultation is now in progress." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting price for consultation {Id}", id);
                return StatusCode(500, new { message = "An error occurred while accepting the price" });
            }
        }

        [HttpPost("{id}/reject-price")]
        [Authorize]
        public async Task<IActionResult> RejectPrice(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _consultationService.RejectPriceAsync(id, userId);
                if (!result)
                    return BadRequest(new { message = "Failed to reject price. Make sure you are the consultation owner and the consultation is awaiting payment." });

                // Notify via SignalR
                await _hubContext.Clients.Group($"consultation_{id}").SendAsync("PriceRejected", new { consultationId = id });

                return Ok(new { message = "Price rejected. The consultation is now open for other experts." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting price for consultation {Id}", id);
                return StatusCode(500, new { message = "An error occurred while rejecting the price" });
            }
        }
    }
}
