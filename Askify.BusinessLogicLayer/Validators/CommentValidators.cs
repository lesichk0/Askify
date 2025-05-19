using Askify.BusinessLogicLayer.DTO;
using FluentValidation;

namespace Askify.BusinessLogicLayer.Validators
{
    public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.PostId)
                .GreaterThan(0).WithMessage("Post ID must be a positive number.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Comment content is required.")
                .MaximumLength(2000).WithMessage("Comment content cannot exceed 2,000 characters.");
        }
    }

    public class UpdateCommentDtoValidator : AbstractValidator<UpdateCommentDto>
    {
        public UpdateCommentDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Comment content is required.")
                .MaximumLength(2000).WithMessage("Comment content cannot exceed 2,000 characters.");
        }
    }
}
