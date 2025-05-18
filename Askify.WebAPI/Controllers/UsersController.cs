using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askify.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
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
            var experts = await _userService.GetExpertsAsync();
            return Ok(experts);
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
    }
}
