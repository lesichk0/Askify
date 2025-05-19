using Askify.BusinessLogicLayer.DTO;
using FluentValidation;

namespace Askify.BusinessLogicLayer.Validators
{
    public class CreateConsultationDtoValidator : AbstractValidator<CreateConsultationDto>
    {
        public CreateConsultationDtoValidator()
        {
            // Remove duplicate validation rules
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required");

            RuleFor(x => x.ExpertId)
                .Null().WithMessage("ExpertId should be null for new consultations.");

            // Add other validation rules as needed
        }
    }

    public class UpdateConsultationDtoValidator : AbstractValidator<UpdateConsultationDto>
    {
        public UpdateConsultationDtoValidator()
        {
            // Only validate the properties that actually exist on your DTO
            // Comment out or remove the rules for Title and Description
            
            // For example, if you have Status:
            RuleFor(x => x.Status)
                .Must(status => status == null || 
                               status == "Pending" || 
                               status == "Accepted" || 
                               status == "Completed" || 
                               status == "Cancelled")
                .WithMessage("Status must be one of: Pending, Accepted, Completed, Cancelled");
            
            // Add rules for other properties that actually exist on your DTO
        }
    }
}
