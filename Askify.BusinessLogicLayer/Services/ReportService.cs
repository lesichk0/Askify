using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using AutoMapper;

namespace Askify.BusinessLogicLayer.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReportService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ReportDto?> GetByIdAsync(int id)
        {
            var report = await _unitOfWork.Reports.GetByIdAsync(id);
            return report != null ? _mapper.Map<ReportDto>(report) : null;
        }

        public async Task<IEnumerable<ReportDto>> GetAllAsync()
        {
            var reports = await _unitOfWork.Reports.GetAllAsync();
            return _mapper.Map<IEnumerable<ReportDto>>(reports);
        }

        public async Task<IEnumerable<ReportDto>> GetByTargetIdAsync(string targetId)
        {
            var reports = await _unitOfWork.Reports.GetReportsByTargetIdAsync(targetId);
            return _mapper.Map<IEnumerable<ReportDto>>(reports);
        }

        public async Task<int> CreateReportAsync(string reporterId, CreateReportDto reportDto)
        {
            var report = _mapper.Map<Report>(reportDto);
            report.ReporterId = reporterId;
            report.Status = "Pending";
            report.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Reports.AddAsync(report);
            await _unitOfWork.CompleteAsync();

            return report.Id;
        }

        public async Task<bool> UpdateReportStatusAsync(int id, UpdateReportDto reportDto)
        {
            var report = await _unitOfWork.Reports.GetByIdAsync(id);
            if (report == null) return false;

            report.Status = reportDto.Status;
            report.ReviewedAt = DateTime.UtcNow;
            
            _unitOfWork.Reports.Update(report);
            return await _unitOfWork.CompleteAsync();
        }
    }
}
