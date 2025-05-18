using Askify.BusinessLogicLayer.DTO;

namespace Askify.BusinessLogicLayer.Interfaces.Services
{
    public interface IPostService
    {
        Task<IEnumerable<PostDto>> GetAllAsync();
        Task<PostDto?> GetByIdAsync(Guid id);
        Task CreateAsync(CreatePostDto dto);
        Task UpdateAsync(Guid id, UpdatePostDto dto);
        Task DeleteAsync(Guid id);
    }
}
