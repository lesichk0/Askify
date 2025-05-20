using Askify.BusinessLogicLayer.DTO;

namespace Askify.BusinessLogicLayer.Interfaces
{
    public interface IPostService
    {
        Task<PostDto?> GetByIdAsync(int id);
        Task<IEnumerable<PostDto>> GetAllAsync();
        Task<IEnumerable<PostDto>> GetByUserIdAsync(string userId);
        Task<int> CreatePostAsync(string userId, CreatePostDto postDto);
        Task<bool> UpdatePostAsync(int id, string userId, UpdatePostDto postDto);
        Task<bool> DeletePostAsync(int id, string userId);
        Task<bool> LikePostAsync(int postId, string userId);
        Task<bool> UnlikePostAsync(int postId, string userId);
        Task<bool> SavePostAsync(int postId, string userId);
        Task<bool> UnsavePostAsync(int postId, string userId);
        Task<IEnumerable<PostDto>> GetSavedPostsAsync(string userId);
        Task<int> GetUserPostsCountAsync(string userId);
    }
}
