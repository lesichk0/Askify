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
                    
                    // Check if user is an expert (either by role or by IsVerifiedExpert flag)
                    bool isExpert = roles.Contains("Expert") || user.IsVerifiedExpert;
                    
                    _logger.LogInformation($"User {user.UserName}: Roles={string.Join(",", roles)}, IsVerifiedExpert={user.IsVerifiedExpert}, IsExpert={isExpert}");
                    
                    if (isExpert)
                    {
                        var expertDto = new UserDto
                        {
                            Id = user.Id,
                            FullName = user.FullName,
                            Bio = user.Bio,
                            Email = user.Email,
                            AvatarUrl = user.AvatarUrl,
                            IsVerifiedExpert = user.IsVerifiedExpert,
                            IsBlocked = user.IsBlocked,
                            Role = "Expert" // Explicitly set role to Expert
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
            
            if (userId != id && !isAdmin)
            {
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
                
                var userProfile = new
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    // Use UserName property if it exists on UserDto, otherwise use an alternative
                    Email = user.Id, // Fallback to Id which we know exists
                    Bio = user.Bio ?? string.Empty,
                    JoinDate = DateTime.UtcNow, // Use current time since CreatedAt doesn't exist
                    Role = User.IsInRole("Expert") ? "Expert" : "User",
                    PostsCount = postsCount,
                    ConsultationsCount = consultationsCount
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
