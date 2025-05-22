using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Askify.BusinessLogicLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }


        public async Task<UserDto?> GetByIdAsync(string id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<IEnumerable<UserDto>> GetExpertsAsync()
        {
            var experts = await _unitOfWork.Users.GetExpertsAsync();
            return _mapper.Map<IEnumerable<UserDto>>(experts);
        }

        public async Task<IEnumerable<UserDto>> SearchUsersAsync(string query)
        {
            var users = await _unitOfWork.Users.SearchUsersAsync(query);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<bool> UpdateUserAsync(string id, UpdateUserDto userDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return false;

            _mapper.Map(userDto, user);
            _unitOfWork.Users.Update(user);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> BlockUserAsync(string id, string reason)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return false;

            user.IsBlocked = true;
            user.BlockReason = reason;
            user.BlockedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> UnblockUserAsync(string id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return false;

            user.IsBlocked = false;
            user.BlockReason = null;
            user.BlockedAt = null;

            _unitOfWork.Users.Update(user);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> VerifyExpertAsync(string id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return false;

            user.IsVerifiedExpert = true;
            user.VerifiedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User"; // Default to "User" if no role found

                var userDto = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Bio = user.Bio,
                    AvatarUrl = user.AvatarUrl,
                    IsVerifiedExpert = user.IsVerifiedExpert,
                    IsBlocked = user.IsBlocked,
                    Role = role, // Make sure role is included
                    Email = user.Email
                };

                userDtos.Add(userDto);
            }

            return userDtos;
        }
    }
    
}
