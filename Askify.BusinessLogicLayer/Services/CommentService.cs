using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using AutoMapper;

namespace Askify.BusinessLogicLayer.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CommentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CommentDto?> GetByIdAsync(int id)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(id);
            return comment != null ? _mapper.Map<CommentDto>(comment) : null;
        }

        public async Task<IEnumerable<CommentDto>> GetByPostIdAsync(int postId)
        {
            var comments = await _unitOfWork.Comments.GetByPostIdAsync(postId);
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public async Task<int> CreateCommentAsync(string userId, CreateCommentDto commentDto)
        {
            var comment = _mapper.Map<Comment>(commentDto);
            comment.AuthorId = userId;
            comment.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.CompleteAsync();

            return comment.Id;
        }

        public async Task<bool> UpdateCommentAsync(int id, string userId, UpdateCommentDto commentDto)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(id);
            if (comment == null || comment.AuthorId != userId) return false;

            _mapper.Map(commentDto, comment);
            _unitOfWork.Comments.Update(comment);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteCommentAsync(int id, string userId)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(id);
            if (comment == null || comment.AuthorId != userId) return false;

            _unitOfWork.Comments.Remove(comment);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> LikeCommentAsync(int commentId, string userId)
        {
            var existing = await _unitOfWork.CommentLikes.FindAsync(cl => cl.CommentId == commentId && cl.UserId == userId);
            if (existing.Any()) return true; // Already liked

            var commentLike = new CommentLike
            {
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CommentLikes.AddAsync(commentLike);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> UnlikeCommentAsync(int commentId, string userId)
        {
            var existing = await _unitOfWork.CommentLikes.FindAsync(cl => cl.CommentId == commentId && cl.UserId == userId);
            if (!existing.Any()) return true; // Not liked

            _unitOfWork.CommentLikes.Remove(existing.First());
            return await _unitOfWork.CompleteAsync();
        }
    }
}
