using Askify.BusinessLogicLayer.DTO;
using FluentValidation;

namespace Askify.BusinessLogicLayer.Validators
{
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            // Only validate FullName if it's provided (allow partial updates)
            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.FullName));

            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters.");
                
            RuleFor(x => x.AvatarUrl)
                .MaximumLength(2000).WithMessage("Avatar URL cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.AvatarUrl));
        }
    }
}
