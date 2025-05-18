using Askify.BusinessLogicLayer.DTO;

namespace Askify.BusinessLogicLayer.Interfaces
{
    public interface ICommentService
    {
        Task<CommentDto?> GetByIdAsync(int id);
        Task<IEnumerable<CommentDto>> GetByPostIdAsync(int postId);
        Task<int> CreateCommentAsync(string userId, CreateCommentDto commentDto);
        Task<bool> UpdateCommentAsync(int id, string userId, UpdateCommentDto commentDto);
        Task<bool> DeleteCommentAsync(int id, string userId);
        Task<bool> LikeCommentAsync(int commentId, string userId);
        Task<bool> UnlikeCommentAsync(int commentId, string userId);
    }
}
