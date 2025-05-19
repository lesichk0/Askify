using Askify.BusinessLogicLayer.DTO;
using FluentValidation;

namespace Askify.BusinessLogicLayer.Validators
{
    public class CreateReportDtoValidator : AbstractValidator<CreateReportDto>
    {
        public CreateReportDtoValidator()
        {
            // Update these to match the actual properties that exist on your DTO
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required.")
                .MaximumLength(1000).WithMessage("Reason cannot exceed 1,000 characters.");
                
            // If your DTO has other properties like ReportType or TargetInfo, validate those instead
        }
    }

    public class UpdateReportDtoValidator : AbstractValidator<UpdateReportDto>
    {
        public UpdateReportDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(status => status == "Pending" || 
                               status == "Approved" || 
                               status == "Rejected")
                .WithMessage("Status must be one of: Pending, Approved, Rejected");
        }
    }
}
