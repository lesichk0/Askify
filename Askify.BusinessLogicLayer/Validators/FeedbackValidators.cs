using Askify.BusinessLogicLayer.DTO;
using FluentValidation;

namespace Askify.BusinessLogicLayer.Validators
{
    public class CreateFeedbackDtoValidator : AbstractValidator<CreateFeedbackDto>
    {
        public CreateFeedbackDtoValidator()
        {
            RuleFor(x => x.ExpertId)
                .NotEmpty().WithMessage("Expert ID is required.");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1,000 characters.");
        }
    }
}
