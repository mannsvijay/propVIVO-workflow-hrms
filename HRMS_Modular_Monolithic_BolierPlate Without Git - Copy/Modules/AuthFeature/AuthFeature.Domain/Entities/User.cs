// ================================================================
// FILE: Modules/AuthFeature/AuthFeature.Domain/Entities/User.cs
// WHY: Domain entity for User — contains auth credentials + role
// ================================================================
using HRMS.Core.Postgres.Common;

namespace AuthFeature.Domain
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public Role? Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }

    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;   // SuperAdmin | HR | Manager | Employee
        public string? Description { get; set; }
    }
}


// ================================================================
// FILE: Modules/AuthFeature/AuthFeature.Application/DTOs/AuthRequest.cs
// WHY: Input/Output DTOs for login, register, refresh token flows
// ================================================================
using HRMS.Core.Postgres.Common;
using HRMS.Shared.Application.DTOs;
using MediatR;

namespace AuthFeature.Application.DTO
{
    // ─── Login ───────────────────────────────────────────────
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest : ExecutionRequest, IRequest<BaseResponse<LoginResponse>>
    {
        public LoginDto? RequestParam { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new();
    }

    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // ─── Refresh Token ────────────────────────────────────────
    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest : ExecutionRequest, IRequest<BaseResponse<LoginResponse>>
    {
        public RefreshTokenDto? RequestParam { get; set; }
    }

    // ─── Change Password ──────────────────────────────────────
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest : ExecutionRequest, IRequest<BaseResponse<bool>>
    {
        public ChangePasswordDto? RequestParam { get; set; }
    }
}


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