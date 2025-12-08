using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Askify.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Askify.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService; // Assuming IPostService is registered
        private readonly IConsultationService _consultationService; // Assuming IConsultationService is registered
        private readonly ILogger<UsersController> _logger; // Assuming ILogger is registered
        private readonly UserManager<User> _userManager;

        public UsersController(IUserService userService, IPostService postService, IConsultationService consultationService, ILogger<UsersController> logger, UserManager<User> userManager)
        {
            _userService = userService;
            _postService = postService;
            _consultationService = consultationService;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize] // Changed from [Authorize(Roles = "Admin")] to allow all authenticated users
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("experts")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetExperts()
        {
            try
            {
                _logger.LogInformation("Fetching experts");
                var users = _userManager.Users.ToList();
                var expertsList = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    bool isExpert = roles.Contains("Expert") || user.IsVerifiedExpert;
                    
                    if (isExpert)
                    {
                        var feedbacks = await _userService.GetFeedbacksForExpertAsync(user.Id);
                        var feedbackList = feedbacks.ToList();
                        double? averageRating = feedbackList.Any() ? feedbackList.Average(f => f.Rating) : null;
                        int reviewsCount = feedbackList.Count;
                        
                        var expertDto = new UserDto
                        {
                            Id = user.Id,
                            FullName = user.FullName,
                            Bio = user.Bio,
                            Email = user.Email,
                            AvatarUrl = user.AvatarUrl,
                            IsVerifiedExpert = user.IsVerifiedExpert,
                            IsBlocked = user.IsBlocked,
                            Role = "Expert",
                            AverageRating = averageRating,
                            ReviewsCount = reviewsCount
                        };
                        
                        expertsList.Add(expertDto);
                    }
                }

                _logger.LogInformation($"Found {expertsList.Count} experts");
                return Ok(expertsList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving experts");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<UserDto>>> Search([FromQuery] string query)
        {
            var users = await _userService.SearchUsersAsync(query);
            return Ok(users);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto userDto)
        {
            // Check if the current user is updating their own profile or is an admin
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            _logger.LogInformation($"Update user request: URL id={id}, Token userId={userId}, isAdmin={isAdmin}");
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID from token is null or empty");
                return Unauthorized();
            }
            
            if (userId != id && !isAdmin)
            {
                _logger.LogWarning($"Forbidden: userId ({userId}) != id ({id}) and not admin");
                return Forbid();
            }
            
            var result = await _userService.UpdateUserAsync(id, userDto);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpPost("{id}/block")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Block(string id, [FromBody] string reason)
        {
            var result = await _userService.BlockUserAsync(id, reason);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpPost("{id}/unblock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unblock(string id)
        {
            var result = await _userService.UnblockUserAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifyExpert(string id)
        {
            var result = await _userService.VerifyExpertAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
                
            try
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                    return NotFound("User not found");
                    
                // Get additional profile information
                var postsCount = await _postService.GetUserPostsCountAsync(userId);
                var consultationsCount = await _consultationService.GetUserConsultationsCountAsync(userId);
                
                // Get rating information for experts
                double? averageRating = null;
                int reviewsCount = 0;
                bool isVerifiedExpert = user.IsVerifiedExpert || User.IsInRole("Expert");
                
                if (isVerifiedExpert)
                {
                    var feedbacks = await _userService.GetFeedbacksForExpertAsync(userId);
                    var feedbackList = feedbacks.ToList();
                    averageRating = feedbackList.Any() ? feedbackList.Average(f => f.Rating) : null;
                    reviewsCount = feedbackList.Count;
                }
                
                var userProfile = new
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Bio = user.Bio ?? string.Empty,
                    JoinDate = DateTime.UtcNow, // Use current time since CreatedAt doesn't exist
                    Role = isVerifiedExpert ? "Expert" : "User",
                    PostsCount = postsCount,
                    ConsultationsCount = consultationsCount,
                    IsVerifiedExpert = isVerifiedExpert,
                    AverageRating = averageRating,
                    ReviewsCount = reviewsCount
                };
                
                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile for ID {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving user profile" });
            }
        }
    }
}
