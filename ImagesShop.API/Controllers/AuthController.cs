using ImagesShop.Application;
using ImagesShop.Application.DTOs.Auth;
using ImagesShop.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ImagesShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly JwtOptions _jwtOptions;

        public AuthController(IAuthService auth, IOptions<JwtOptions> jwtOptions)
        {
            _auth = auth;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterStep1RequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _auth.RegisterAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("complete-registration")]
        [AllowAnonymous]
        public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _auth.CompleteRegistrationAsync(request, cancellationToken);
            AppendRefreshCookie(result.RefreshToken);
            return Ok(result);
        }

        [HttpPost("resend-verification-code")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationCodeRequestDTO request, CancellationToken cancellationToken)
        {
            await _auth.ResendVerificationCodeAsync(request, cancellationToken);
            return NoContent();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _auth.LoginAsync(request, cancellationToken);
            AppendRefreshCookie(result.RefreshToken);
            return Ok(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrWhiteSpace(refreshToken)) return Unauthorized("Refresh token is missing");

            var result = await _auth.RefreshAsync(refreshToken, cancellationToken);
            AppendRefreshCookie(result.RefreshToken);
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _auth.LogoutAsync(refreshToken, cancellationToken);
                Response.Cookies.Delete("refreshToken", BuildCookieOptions(DateTimeOffset.UtcNow.AddDays(-1)));
            }

            return NoContent();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO request, CancellationToken cancellationToken)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

            await _auth.ChangePasswordAsync(userId, request, cancellationToken);
            return NoContent();
        }

        private void AppendRefreshCookie(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return;
            var options = BuildCookieOptions(DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays));
            Response.Cookies.Append("refreshToken", refreshToken, options);
        }

        private static CookieOptions BuildCookieOptions(DateTimeOffset expires) => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expires,
            Path = "/"
        };
    }
}
