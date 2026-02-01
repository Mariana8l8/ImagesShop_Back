using ImagesShop.Application;
using ImagesShop.Application.DTOs.Auth;
using ImagesShop.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _auth.RegisterAsync(request, cancellationToken);
            AppendRefreshCookie(result.RefreshToken);
            return Ok(result);
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
