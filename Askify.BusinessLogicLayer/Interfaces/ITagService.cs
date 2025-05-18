using Askify.BusinessLogicLayer.DTO;

namespace Askify.BusinessLogicLayer.Interfaces
{
    public interface ITagService
    {
        Task<TagDto?> GetByIdAsync(int id);
        Task<TagDto?> GetByNameAsync(string name);
        Task<IEnumerable<TagDto>> GetAllAsync();
        Task<int> CreateTagAsync(string name);
    }
}
