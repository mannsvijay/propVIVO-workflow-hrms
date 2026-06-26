
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