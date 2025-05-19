using Askify.BusinessLogicLayer.DTO;
using FluentValidation;

namespace Askify.BusinessLogicLayer.Validators
{
    public class CreatePostDtoValidator : AbstractValidator<CreatePostDto>
    {
        public CreatePostDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.")
                .MaximumLength(10000).WithMessage("Content cannot exceed 10,000 characters.");

            RuleFor(x => x.Tags)
                .Must(tags => tags == null || tags.Count <= 10)
                .WithMessage("A post cannot have more than 10 tags.");

            RuleForEach(x => x.Tags)
                .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters.");
        }
    }

    public class UpdatePostDtoValidator : AbstractValidator<UpdatePostDto>
    {
        public UpdatePostDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.")
                .MaximumLength(10000).WithMessage("Content cannot exceed 10,000 characters.");

            RuleFor(x => x.Tags)
                .Must(tags => tags == null || tags.Count <= 10)
                .WithMessage("A post cannot have more than 10 tags.");

            RuleForEach(x => x.Tags)
                .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters.");
        }
    }
}
