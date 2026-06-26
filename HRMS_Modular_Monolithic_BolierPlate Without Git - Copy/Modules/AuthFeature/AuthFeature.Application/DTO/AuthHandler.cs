// WHY: MediatR handlers for Login and RefreshToken — the core business logic
using AuthFeature.Application.Repository;
using AuthFeature.Application.Services;
using HRMS.Core.Telemetry.Exceptions;
using HRMS.Shared.Application.Constants;
using HRMS.Shared.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
 
namespace AuthFeature.Application.DTO
{
    // ─── Login Handler ────────────────────────────────────────
    public class LoginHandler : IRequestHandler<LoginRequest, BaseResponse<LoginResponse>>
    {
        private readonly IAuthRepository _authRepo;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;
 
        public LoginHandler(IAuthRepository authRepo, IJwtService jwtService, IPasswordService passwordService)
        {
            _authRepo = authRepo;
            _jwtService = jwtService;
            _passwordService = passwordService;
        }
 
        public async Task<BaseResponse<LoginResponse>> Handle(LoginRequest request, CancellationToken ct)
        {
            if (request?.RequestParam == null)
                throw new BadRequestException("Invalid request");
 
            var dto = request.RequestParam;
 
            // 1. Find user by email
            var user = await _authRepo.GetByEmailAsync(dto.Email)
                ?? throw new BadRequestException("Invalid email or password");
 
            if (!user.IsActive)
                throw new ForbiddenException("Account is deactivated. Contact HR.");
 
            // 2. Verify password
            if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
                throw new BadRequestException("Invalid email or password");
 
            // 3. Generate tokens
            var roleName = user.Role?.Name ?? "Employee";
            var accessToken  = _jwtService.GenerateAccessToken(user.Id, user.Email, roleName);
            var refreshToken = _jwtService.GenerateRefreshToken();
 
            // 4. Persist refresh token (7 day expiry)
            await _authRepo.UpdateRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));
            await _authRepo.UpdateLastLoginAsync(user.Id);
 
            // 5. Build response
            var response = new LoginResponse
            {
                AccessToken  = accessToken,
                RefreshToken = refreshToken,
                User = new UserInfo
                {
                    Id    = user.Id,
                    Email = user.Email,
                    Name  = user.Email.Split('@')[0], // fallback name
                    Role  = roleName
                }
            };
 
            return new BaseResponse<LoginResponse>
            {
                Data       = response,
                StatusCode = StatusCodes.Status200OK,
                Message    = "Login successful",
                Success    = true
            };
        }
    }
 
    // ─── Refresh Token Handler ────────────────────────────────
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, BaseResponse<LoginResponse>>
    {
        private readonly IAuthRepository _authRepo;
        private readonly IJwtService _jwtService;
 
        public RefreshTokenHandler(IAuthRepository authRepo, IJwtService jwtService)
        {
            _authRepo = authRepo;
            _jwtService = jwtService;
        }
 
        public async Task<BaseResponse<LoginResponse>> Handle(RefreshTokenRequest request, CancellationToken ct)
        {
            if (request?.RequestParam == null)
                throw new BadRequestException("Invalid request");
 
            var user = await _authRepo.GetByRefreshTokenAsync(request.RequestParam.RefreshToken)
                ?? throw new BadRequestException("Invalid or expired refresh token");
 
            if (user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                await _authRepo.UpdateRefreshTokenAsync(user.Id, null, null);
                throw new BadRequestException("Refresh token expired. Please log in again.");
            }
 
            var roleName = user.Role?.Name ?? "Employee";
            var accessToken  = _jwtService.GenerateAccessToken(user.Id, user.Email, roleName);
            var refreshToken = _jwtService.GenerateRefreshToken();
 
            await _authRepo.UpdateRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));
 
            return new BaseResponse<LoginResponse>
            {
                Data = new LoginResponse
                {
                    AccessToken  = accessToken,
                    RefreshToken = refreshToken,
                    User = new UserInfo { Id = user.Id, Email = user.Email, Role = roleName }
                },
                StatusCode = StatusCodes.Status200OK,
                Success    = true
            };
        }
    }
}
 