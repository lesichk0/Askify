using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Askify.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            if (result == null || !result.IsSuccess)
            {
                return BadRequest(result?.Message ?? "Login failed");
            }

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate role is either "User" or "Expert"
            if (!string.IsNullOrEmpty(registerDto?.Role) && registerDto.Role != "User" && registerDto.Role != "Expert")
            {
                return BadRequest(new { message = "Invalid role. Role must be either 'User' or 'Expert'." });
            }
            
            // Default to "User" if no role provided
            string role = string.IsNullOrEmpty(registerDto.Role) ? "User" : registerDto.Role;
            
            try
            {
                var result = await _authService.RegisterAsync(
                    registerDto
                );

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Use the exception or log it
                //_logger?.LogError(ex, "Error during user registration");
                return StatusCode(500, new { message = "An error occurred during registration." });
            }
        }
    }
}
