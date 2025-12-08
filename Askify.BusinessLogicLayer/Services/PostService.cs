using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using AutoMapper;

namespace Askify.BusinessLogicLayer.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PostService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PostDto?> GetByIdAsync(int id)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            return post != null ? _mapper.Map<PostDto>(post) : null;
        }

        public async Task<IEnumerable<PostDto>> GetAllAsync()
        {
            var posts = await _unitOfWork.Posts.GetPostsWithAuthorAsync();
            var postDtos = _mapper.Map<IEnumerable<PostDto>>(posts);
            
            // Ensure author names are populated
            foreach (var postDto in postDtos)
            {
                if (string.IsNullOrEmpty(postDto.AuthorName))
                {
                    var author = await _unitOfWork.Users.GetByIdAsync(postDto.AuthorId);
                    if (author != null)
                    {
                        postDto.AuthorName = author.FullName;
                    }
                }
            }
            
            return postDtos;
        }

        public async Task<IEnumerable<PostDto>> GetByUserIdAsync(string userId)
        {
            var posts = await _unitOfWork.Posts.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<PostDto>>(posts);
        }

        public async Task<int> CreatePostAsync(string userId, CreatePostDto postDto)
        {
            var post = _mapper.Map<Post>(postDto);
            post.AuthorId = userId;
            post.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Posts.AddAsync(post);
            await _unitOfWork.CompleteAsync();

            // Handle tags
            if (postDto.Tags.Any())
            {
                foreach (var tagName in postDto.Tags)
                {
                    var tag = await _unitOfWork.Tags.GetByNameAsync(tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        await _unitOfWork.Tags.AddAsync(tag);
                        await _unitOfWork.CompleteAsync();
                    }

                    var postTag = new PostTag
                    {
                        PostId = post.Id,
                        TagId = tag.Id
                    };
                    await _unitOfWork.PostTags.AddAsync(postTag);
                }
                await _unitOfWork.CompleteAsync();
            }

            return post.Id;
        }

        public async Task<bool> UpdatePostAsync(int id, string userId, UpdatePostDto postDto)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null || post.AuthorId != userId) return false;

            _mapper.Map(postDto, post);
            _unitOfWork.Posts.Update(post);

            // Update tags if needed
            if (postDto.Tags != null)
            {
                // Remove existing tags
                var existingPostTags = await _unitOfWork.PostTags.FindAsync(pt => pt.PostId == id);
                _unitOfWork.PostTags.RemoveRange(existingPostTags);

                // Add new tags
                foreach (var tagName in postDto.Tags)
                {
                    var tag = await _unitOfWork.Tags.GetByNameAsync(tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        await _unitOfWork.Tags.AddAsync(tag);
                        await _unitOfWork.CompleteAsync();
                    }

                    var postTag = new PostTag
                    {
                        PostId = post.Id,
                        TagId = tag.Id
                    };
                    await _unitOfWork.PostTags.AddAsync(postTag);
                }
            }

            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeletePostAsync(int id, string userId)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null || post.AuthorId != userId) return false;

            _unitOfWork.Posts.Remove(post);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> LikePostAsync(int postId, string userId)
        {
            var existing = await _unitOfWork.PostLikes.FindAsync(pl => pl.PostId == postId && pl.UserId == userId);
            if (existing.Any()) return true; // Already liked

            var postLike = new PostLike
            {
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.PostLikes.AddAsync(postLike);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> UnlikePostAsync(int postId, string userId)
        {
            var existing = await _unitOfWork.PostLikes.FindAsync(pl => pl.PostId == postId && pl.UserId == userId);
            if (!existing.Any()) return true; // Not liked

            _unitOfWork.PostLikes.Remove(existing.First());
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> SavePostAsync(int postId, string userId)
        {
            var existing = await _unitOfWork.SavedPosts.FindAsync(sp => sp.PostId == postId && sp.UserId == userId);
            if (existing.Any()) return true; // Already saved

            var savedPost = new SavedPost
            {
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.SavedPosts.AddAsync(savedPost);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> UnsavePostAsync(int postId, string userId)
        {
            var existing = await _unitOfWork.SavedPosts.FindAsync(sp => sp.PostId == postId && sp.UserId == userId);
            if (!existing.Any()) return true; // Not saved

            _unitOfWork.SavedPosts.Remove(existing.First());
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<PostDto>> GetSavedPostsAsync(string userId)
        {
            var savedPosts = await _unitOfWork.SavedPosts.GetSavedPostsForUserAsync(userId);
            var posts = savedPosts.Select(sp => sp.Post);
            return _mapper.Map<IEnumerable<PostDto>>(posts);
        }

        public async Task<int> GetUserPostsCountAsync(string userId)
        {
            var posts = await _unitOfWork.Posts.FindAsync(p => p.AuthorId == userId);
            return posts.Count();
        }

        public async Task<int> GetLikesCountAsync(int postId)
        {
            var likes = await _unitOfWork.PostLikes.FindAsync(pl => pl.PostId == postId);
            return likes.Count();
        }

        public async Task<int> GetCommentsCountAsync(int postId)
        {
            var comments = await _unitOfWork.Comments.FindAsync(c => c.PostId == postId);
            return comments.Count();
        }

        public async Task<bool> IsLikedByUserAsync(int postId, string userId)
        {
            var likes = await _unitOfWork.PostLikes.FindAsync(pl => pl.PostId == postId && pl.UserId == userId);
            return likes.Any();
        }

        public async Task<bool> IsSavedByUserAsync(int postId, string userId)
        {
            var saved = await _unitOfWork.SavedPosts.FindAsync(sp => sp.PostId == postId && sp.UserId == userId);
            return saved.Any();
        }

        public async Task<PostDto?> GetByIdWithUserContextAsync(int id, string? userId)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null) return null;

            var postDto = _mapper.Map<PostDto>(post);
            postDto.LikesCount = await GetLikesCountAsync(id);
            postDto.CommentsCount = await GetCommentsCountAsync(id);

            if (!string.IsNullOrEmpty(userId))
            {
                postDto.IsLikedByCurrentUser = await IsLikedByUserAsync(id, userId);
                postDto.IsSavedByCurrentUser = await IsSavedByUserAsync(id, userId);
            }

            return postDto;
        }
    }
}
