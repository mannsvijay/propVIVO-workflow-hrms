 
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
 