using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askify.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetById(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null) 
                return NotFound(new { Message = $"Comment with ID {id} was not found." });
            
            return Ok(comment);
        }

        [HttpGet("post/{postId}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetByPostId(int postId)
        {
            var comments = await _commentService.GetByPostIdAsync(postId);
            return Ok(comments);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<int>> Create([FromBody] CreateCommentDto commentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var commentId = await _commentService.CreateCommentAsync(userId, commentDto);
            return CreatedAtAction(nameof(GetById), new { id = commentId }, commentId);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentDto commentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _commentService.UpdateCommentAsync(id, userId, commentDto);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _commentService.DeleteCommentAsync(id, userId);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpPost("{id}/like")]
        [Authorize]
        public async Task<IActionResult> Like(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _commentService.LikeCommentAsync(id, userId);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpDelete("{id}/like")]
        [Authorize]
        public async Task<IActionResult> Unlike(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _commentService.UnlikeCommentAsync(id, userId);
            if (!result) return BadRequest();
            return Ok();
        }
    }
}
