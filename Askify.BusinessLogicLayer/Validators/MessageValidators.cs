using Askify.BusinessLogicLayer.DTO;
using FluentValidation;

namespace Askify.BusinessLogicLayer.Validators
{
    public class CreateMessageDtoValidator : AbstractValidator<CreateMessageDto>
    {
        public CreateMessageDtoValidator()
        {
            RuleFor(x => x.ReceiverId)
                .NotEmpty().WithMessage("Receiver ID is required.");

            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Message text is required.")
                .MaximumLength(5000).WithMessage("Message text cannot exceed 5,000 characters.");
        }
    }
}
