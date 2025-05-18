using Askify.BusinessLogicLayer.DTO;

namespace Askify.BusinessLogicLayer.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(string id);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<IEnumerable<UserDto>> GetExpertsAsync();
        Task<IEnumerable<UserDto>> SearchUsersAsync(string query);
        Task<bool> UpdateUserAsync(string id, UpdateUserDto userDto);
        Task<bool> BlockUserAsync(string id, string reason);
        Task<bool> UnblockUserAsync(string id);
        Task<bool> VerifyExpertAsync(string id);
    }
}
