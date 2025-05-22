using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Askify.BusinessLogicLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        // Update the Login method to handle JWT errors more gracefully
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Invalid email or password"
                    };
                }

                var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
                if (!result)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Invalid email or password"
                    };
                }

                if (user.IsBlocked)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = $"Your account is blocked. Reason: {user.BlockReason}"
                    };
                }

                var userRoles = await _userManager.GetRolesAsync(user);

                // Make sure we check for null JWT key before attempting to generate token
                if (string.IsNullOrEmpty(_configuration["Jwt:Key"]))
                {
                    throw new InvalidOperationException("Authentication service is misconfigured. Please contact support.");
                }

                var token = GenerateJwtToken(user.Id, userRoles);

                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Token = token,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Roles = userRoles
                };
            }
            catch (Exception) {
                // Consider logging the exception here
                throw;
            }
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email already exists
            var emailExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (emailExists != null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Email already exists"
                };
            }

            // Create new user
            var user = new User
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                CreatedAt = DateTime.UtcNow,
                // Set IsVerifiedExpert to true if role is Expert
                IsVerifiedExpert = registerDto.Role?.Equals("Expert", StringComparison.OrdinalIgnoreCase) ?? false
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            // Normalize the role to ensure consistency
            string role = registerDto.Role?.Equals("Expert", StringComparison.OrdinalIgnoreCase) ?? false 
                ? "Expert" 
                : "User";
            
            // Add user to role
            await _userManager.AddToRoleAsync(user, role);
            
            // Confirm the role was added correctly (for debugging)
            var roles = await _userManager.GetRolesAsync(user);
            //_logger.LogInformation($"User {user.Email} was assigned roles: {string.Join(", ", roles)}");

            // Generate token for the new user
            var userRoles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user.Id, userRoles);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Token = token,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Roles = userRoles
            };
        }

        // Replace the async method without awaits with a synchronous version
        public string GenerateJwtToken(string userId, IList<string> roles)
        {
            // Get the key from configuration and verify it exists
            var jwtKey = _configuration["Jwt:Key"] ?? 
                throw new InvalidOperationException("JWT Key is not configured");
            
            // Create signing credentials with null check
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            // Create claims list
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            
            // Add roles to claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            // Create and return the token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30), // 30-day expiration
                signingCredentials: creds
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Fix missing GenerateJwtTokenAsync implementation
        public async Task<string> GenerateJwtTokenAsync(string userId, IList<string> roles)
        {
            // Add actual implementation with at least one await
            var jwtKey = _configuration["Jwt:Key"] ?? 
                throw new InvalidOperationException("JWT Key is not configured");
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            // Add at least one await operation
            await Task.Yield();
            
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
