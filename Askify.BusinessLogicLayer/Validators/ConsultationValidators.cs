using Askify.BusinessLogicLayer.DTO;
using FluentValidation;

namespace Askify.BusinessLogicLayer.Validators
{
    public class CreateConsultationDtoValidator : AbstractValidator<CreateConsultationDto>
    {
        public CreateConsultationDtoValidator()
        {
            RuleFor(x => x.ExpertId)
                .Null().WithMessage("ExpertId should be null for new consultations.");

            // Only validate the properties that actually exist on your DTO
            // Comment out or remove the rules for Title and Description

            // For example, if you have ExpertId:
            RuleFor(x => x.ExpertId)
                .Null().WithMessage("ExpertId should be null for new consultations.");

            // If you have other properties, add rules for them
            // For example:
            // RuleFor(x => x.Message)
            //     .NotEmpty().WithMessage("Message is required.")
            //     .MaximumLength(1000).WithMessage("Message cannot exceed 1,000 characters.");
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
