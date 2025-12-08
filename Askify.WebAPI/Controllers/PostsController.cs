using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askify.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetAll()
        {
            var posts = await _postService.GetAllAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var post = await _postService.GetByIdWithUserContextAsync(id, userId);
            if (post == null) return NotFound();
            return Ok(post);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetByUserId(string userId)
        {
            var posts = await _postService.GetByUserIdAsync(userId);
            return Ok(posts);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<int>> Create([FromBody] CreatePostDto postDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var postId = await _postService.CreatePostAsync(userId, postDto);
            return CreatedAtAction(nameof(GetById), new { id = postId }, postId);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePostDto postDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _postService.UpdatePostAsync(id, userId, postDto);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _postService.DeletePostAsync(id, userId);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpPost("{id}/like")]
        [Authorize]
        public async Task<IActionResult> Like(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _postService.LikePostAsync(id, userId);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpDelete("{id}/like")]
        [Authorize]
        public async Task<IActionResult> Unlike(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _postService.UnlikePostAsync(id, userId);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpPost("{id}/save")]
        [Authorize]
        public async Task<IActionResult> Save(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _postService.SavePostAsync(id, userId);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpDelete("{id}/save")]
        [Authorize]
        public async Task<IActionResult> Unsave(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _postService.UnsavePostAsync(id, userId);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpGet("saved")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetSaved()
        {
            // Fix: Get userId from ClaimTypes.NameIdentifier instead of "sub"
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var posts = await _postService.GetSavedPostsAsync(userId);
            return Ok(posts);
        }
    }
}
