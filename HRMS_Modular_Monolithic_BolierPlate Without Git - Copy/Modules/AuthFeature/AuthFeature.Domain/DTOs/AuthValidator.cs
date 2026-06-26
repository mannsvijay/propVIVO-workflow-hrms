
// ================================================================
// FILE: Modules/AuthFeature/AuthFeature.Application/DTOs/AuthValidator.cs
// WHY: FluentValidation rules for login request
// ================================================================
using FluentValidation;
 
namespace AuthFeature.Application.DTO
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");
 
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");
        }
    }
 
    public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }
}
 