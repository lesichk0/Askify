using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askify.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        [Authorize] // Changed from [Authorize(Roles = "Admin")] to allow all authenticated users
        public async Task<ActionResult<IEnumerable<ReportDto>>> GetAll()
        {
            var reports = await _reportService.GetAllAsync();
            return Ok(reports);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReportDto>> GetById(int id)
        {
            var report = await _reportService.GetByIdAsync(id);
            if (report == null) return NotFound();
            return Ok(report);
        }

        [HttpGet("target/{targetId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReportDto>>> GetByTargetId(string targetId)
        {
            var reports = await _reportService.GetByTargetIdAsync(targetId);
            return Ok(reports);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<int>> Create([FromBody] CreateReportDto reportDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var reportId = await _reportService.CreateReportAsync(userId, reportDto);
            return CreatedAtAction(nameof(GetById), new { id = reportId }, reportId);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Keep Admin-only for updating report status
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReportDto reportDto)
        {
            var result = await _reportService.UpdateReportStatusAsync(id, reportDto);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
