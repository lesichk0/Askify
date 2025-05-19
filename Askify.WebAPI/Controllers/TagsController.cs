using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Askify.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetAll()
        {
            var tags = await _tagService.GetAllAsync();
            return Ok(tags);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TagDto>> GetById(int id)
        {
            var tag = await _tagService.GetByIdAsync(id);
            if (tag == null) return NotFound();
            return Ok(tag);
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<TagDto>> GetByName(string name)
        {
            var tag = await _tagService.GetByNameAsync(name);
            if (tag == null) return NotFound();
            return Ok(tag);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<int>> Create([FromBody] string name)
        {
            var tagId = await _tagService.CreateTagAsync(name);
            return CreatedAtAction(nameof(GetById), new { id = tagId }, tagId);
        }
    }
}
