using Askify.BusinessLogicLayer.DTO;

namespace Askify.BusinessLogicLayer.Interfaces
{
    public interface IReportService
    {
        Task<ReportDto?> GetByIdAsync(int id);
        Task<IEnumerable<ReportDto>> GetAllAsync();
        Task<IEnumerable<ReportDto>> GetByTargetIdAsync(string targetId);
        Task<int> CreateReportAsync(string reporterId, CreateReportDto reportDto);
        Task<bool> UpdateReportStatusAsync(int id, UpdateReportDto reportDto);
    }
}
