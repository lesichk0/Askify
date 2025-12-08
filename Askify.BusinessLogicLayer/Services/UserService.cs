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
            if (user == null) return null;
            
            var userDto = _mapper.Map<UserDto>(user);
            
            // Calculate rating for experts
            if (user.IsVerifiedExpert)
            {
                var feedbacks = await _unitOfWork.Feedbacks.GetForExpertAsync(user.Id);
                var feedbackList = feedbacks.ToList();
                userDto.AverageRating = feedbackList.Any() ? feedbackList.Average(f => f.Rating) : null;
                userDto.ReviewsCount = feedbackList.Count;
            }
            
            return userDto;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var userDtos = new List<UserDto>();
            
            foreach (var user in users)
            {
                var userDto = _mapper.Map<UserDto>(user);
                
                // Calculate rating for experts
                if (user.IsVerifiedExpert)
                {
                    var feedbacks = await _unitOfWork.Feedbacks.GetForExpertAsync(user.Id);
                    var feedbackList = feedbacks.ToList();
                    userDto.AverageRating = feedbackList.Any() ? feedbackList.Average(f => f.Rating) : null;
                    userDto.ReviewsCount = feedbackList.Count;
                }
                
                userDtos.Add(userDto);
            }
            
            return userDtos;
        }

        public async Task<IEnumerable<UserDto>> GetExpertsAsync()
        {
            var experts = await _unitOfWork.Users.GetExpertsAsync();
            var expertDtos = new List<UserDto>();
            
            foreach (var expert in experts)
            {
                var userDto = _mapper.Map<UserDto>(expert);
                
                // Calculate rating for each expert
                var feedbacks = await _unitOfWork.Feedbacks.GetForExpertAsync(expert.Id);
                var feedbackList = feedbacks.ToList();
                userDto.AverageRating = feedbackList.Any() ? feedbackList.Average(f => f.Rating) : null;
                userDto.ReviewsCount = feedbackList.Count;
                
                expertDtos.Add(userDto);
            }
            
            return expertDtos;
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
                var role = roles.FirstOrDefault() ?? "User";

                // Get expert's average rating and reviews count
                var feedbacks = await _unitOfWork.Feedbacks.GetForExpertAsync(user.Id);
                var feedbackList = feedbacks.ToList();
                double? averageRating = feedbackList.Any() ? feedbackList.Average(f => f.Rating) : null;
                int reviewsCount = feedbackList.Count;

                var userDto = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Bio = user.Bio,
                    AvatarUrl = user.AvatarUrl,
                    IsVerifiedExpert = user.IsVerifiedExpert,
                    IsBlocked = user.IsBlocked,
                    Role = role,
                    Email = user.Email,
                    AverageRating = averageRating,
                    ReviewsCount = reviewsCount
                };

                userDtos.Add(userDto);
            }

            return userDtos;
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksForExpertAsync(string expertId)
        {
            return await _unitOfWork.Feedbacks.GetForExpertAsync(expertId);
        }
    }
    
}
