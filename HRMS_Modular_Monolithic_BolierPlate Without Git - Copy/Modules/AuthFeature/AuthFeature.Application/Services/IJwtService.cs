// ================================================================
// FILE: Modules/AuthFeature/AuthFeature.Application/Services/IJwtService.cs
// WHY: Interface for JWT token generation/validation — depends on nothing external
// ================================================================
namespace AuthFeature.Application.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(string userId, string email, string role);
        string GenerateRefreshToken();
        (string userId, string email, string role)? ValidateAccessToken(string token);
    }
}


// ================================================================
// FILE: Modules/AuthFeature/AuthFeature.Application/Repository/IAuthRepository.cs
// WHY: Repository interface following the same pattern as ITodoRepository
// ================================================================
using AuthFeature.Domain;
using HRMS.Core.Postgres.Repositories;

namespace AuthFeature.Application.Repository
{
    public interface IAuthRepository : IPostgresRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task UpdateRefreshTokenAsync(string userId, string? refreshToken, DateTime? expiry);
        Task UpdateLastLoginAsync(string userId);
        Task<Role?> GetRoleByIdAsync(string roleId);
        Task<IEnumerable<Role>> GetAllRolesAsync();
    }
}


// ================================================================
// FILE: Modules/AuthFeature/AuthFeature.Application/DTO/AuthHandler.cs
// WHY: MediatR handlers for Login and RefreshToken — the core business logic
// ================================================================
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


// ================================================================
// FILE: Modules/AuthFeature/AuthFeature.Infrastructure/JwtService.cs
// WHY: Concrete JWT implementation using System.IdentityModel.Tokens.Jwt
// INTERVIEW: Explains how JWT works — header.payload.signature (HMACSHA256)
// ================================================================
using AuthFeature.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthFeature.Infrastructure
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config) => _config = config;

        public string GenerateAccessToken(string userId, string email, string role)
        {
            var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
            var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:AccessTokenMinutes"] ?? "60"));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email,          email),
                new Claim(ClaimTypes.Role,           role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:   _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims:   claims,
                expires:  expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public (string userId, string email, string role)? ValidateAccessToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key     = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!);
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(key),
                    ValidateIssuer           = true,
                    ValidIssuer              = _config["Jwt:Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = _config["Jwt:Audience"],
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                }, out var validated);

                var jwt    = (JwtSecurityToken)validated;
                var userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var email  = jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value;
                var role   = jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value;
                return (userId, email, role);
            }
            catch
            {
                return null;
            }
        }
    }


    // ================================================================
    // FILE: Modules/AuthFeature/AuthFeature.Infrastructure/PasswordService.cs
    // WHY: BCrypt password hashing — never store plain text passwords
    // INTERVIEW: Why BCrypt? Adaptive cost factor, built-in salt, replay protection
    // ================================================================
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool   VerifyPassword(string password, string hash);
    }

    public class PasswordService : IPasswordService
    {
        private const int WorkFactor = 12;  // 2^12 rounds — ~300ms per hash

        public string HashPassword(string password)  => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        public bool   VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
    }


    // ================================================================
    // FILE: Modules/AuthFeature/AuthFeature.Infrastructure/AuthRepository.cs
    // WHY: EF Core implementation — follows exact same pattern as TodoRepository
    // ================================================================
    public class UserEntityConfigurator : HRMS.Core.Postgres.Interfaces.IPostgresEntityConfigurator
    {
        public void Configure(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthFeature.Domain.User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(128);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(512);
                entity.Property(e => e.RoleId).IsRequired().HasMaxLength(128);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.DocumentType);
                entity.HasIndex(e => e.RoleId);
                entity.HasOne(e => e.Role)
                    .WithMany()
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AuthFeature.Domain.Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(128);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });
        }
    }

    public class AuthRepository : HRMS.Core.Postgres.Repositories.PostgresDbRepository<AuthFeature.Domain.User>,
        AuthFeature.Application.Repository.IAuthRepository
    {
        private readonly HRMS.Core.Postgres.Data.PostgresDbContext _ctx;

        public AuthRepository(
            HRMS.Core.Postgres.Data.PostgresDbContext context,
            Microsoft.Extensions.Logging.ILogger<AuthRepository> logger,
            HRMS.Core.Telemetry.ITelemetryService telemetryService,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(context, logger, telemetryService, httpContextAccessor)
        {
            _ctx = context;
        }

        public override string TableName => "Users";
        public override string GenerateId(AuthFeature.Domain.User entity) => Guid.NewGuid().ToString();

        public async Task<AuthFeature.Domain.User?> GetByEmailAsync(string email)
            => await _ctx.Set<AuthFeature.Domain.User>()
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.DocumentType == "User");

        public async Task<AuthFeature.Domain.User?> GetByRefreshTokenAsync(string refreshToken)
            => await _ctx.Set<AuthFeature.Domain.User>()
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.DocumentType == "User");

        public async Task UpdateRefreshTokenAsync(string userId, string? refreshToken, DateTime? expiry)
        {
            var user = await _ctx.Set<AuthFeature.Domain.User>().FindAsync(userId);
            if (user == null) return;
            user.RefreshToken       = refreshToken;
            user.RefreshTokenExpiry = expiry;
            user.ModifiedOn         = DateTime.UtcNow;
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdateLastLoginAsync(string userId)
        {
            var user = await _ctx.Set<AuthFeature.Domain.User>().FindAsync(userId);
            if (user == null) return;
            user.LastLoginAt = DateTime.UtcNow;
            user.ModifiedOn  = DateTime.UtcNow;
            await _ctx.SaveChangesAsync();
        }

        public async Task<AuthFeature.Domain.Role?> GetRoleByIdAsync(string roleId)
            => await _ctx.Set<AuthFeature.Domain.Role>().AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);

        public async Task<IEnumerable<AuthFeature.Domain.Role>> GetAllRolesAsync()
            => await _ctx.Set<AuthFeature.Domain.Role>().AsNoTracking().ToListAsync();
    }
}


// ================================================================
// FILE: API/HRMS.API/Controllers/AuthController.cs
// WHY: The REST endpoint the frontend calls (/auth/login, /auth/refresh)
// This is a standard REST controller, NOT GraphQL, because auth
// should be simple HTTP, not schema-driven.
// INTERVIEW: Why not GraphQL for auth? Auth is stateless, mutation-heavy,
// and browsers/mobile handle cookies + Basic/Bearer easier on REST.
// ================================================================
using AuthFeature.Application.DTO;
using HRMS.Shared.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator) => _mediator = mediator;

        /// <summary>Login with email + password. Returns access + refresh tokens.</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(BaseResponse<LoginResponse>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _mediator.Send(new LoginRequest { RequestParam = dto });
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        /// <summary>Exchange a valid refresh token for a new access token.</summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(BaseResponse<LoginResponse>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            var result = await _mediator.Send(new RefreshTokenRequest { RequestParam = dto });
            if (!result.Success) return Unauthorized(result);
            return Ok(result);
        }

        /// <summary>Logout — invalidates the refresh token server-side.</summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            // Invalidate refresh token
            await _mediator.Send(new RefreshTokenRequest
            {
                RequestParam = new RefreshTokenDto { RefreshToken = dto.RefreshToken }
            });
            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>Health check for auth service.</summary>
        [HttpGet("health")]
        public IActionResult Health() => Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    }
}


// ================================================================
// FILE: API/HRMS.API/Extensions/JwtExtensions.cs  (ADD to existing Extensions folder)
// WHY: Wire JWT bearer validation into the ASP.NET pipeline
// Call this in Startup.ConfigureServices BEFORE AddGraphQL
// ================================================================
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HRMS.API.Extensions
{
    public static class JwtExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services, IConfiguration config)
        {
            var secretKey = config["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey is not configured");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer           = true,
                    ValidIssuer              = config["Jwt:Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = config["Jwt:Audience"],
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero  // No tolerance for expired tokens
                };

                // Allow JWT from GraphQL requests too
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var authHeader = ctx.Request.Headers["Authorization"].FirstOrDefault();
                        if (authHeader?.StartsWith("Bearer ") == true)
                            ctx.Token = authHeader["Bearer ".Length..].Trim();
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAuth",         p => p.RequireAuthenticatedUser());
                options.AddPolicy("RequireHR",           p => p.RequireRole("SuperAdmin", "HR"));
                options.AddPolicy("RequireManager",      p => p.RequireRole("SuperAdmin", "HR", "Manager"));
                options.AddPolicy("RequireSuperAdmin",   p => p.RequireRole("SuperAdmin"));
            });

            return services;
        }
    }
}


// ================================================================
// FILE: Modules/AuthFeature/AuthFeature.Infrastructure/ConfigureServiceExtension.cs
// WHY: Register all AuthFeature services in DI container
// ================================================================
using AuthFeature.Application.Repository;
using AuthFeature.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AuthFeature.Infrastructure
{
    public static class ConfigureServiceExtension
    {
        public static IServiceCollection AddAuthFeature(this IServiceCollection services)
        {
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordService, PasswordService>();
            return services;
        }
    }
}


// ================================================================
// APPSETTINGS additions — add to appsettings.json under existing keys
// ================================================================
/*
  "Jwt": {
    "SecretKey": "propVivo-HRMS-SuperSecret-Key-At-Least-32-Chars-Long!",
    "Issuer": "propvivo-hrms-api",
    "Audience": "propvivo-hrms-frontend",
    "AccessTokenMinutes": "60"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://your-production-domain.com"]
  }
*/