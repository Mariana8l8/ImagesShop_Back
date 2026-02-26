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
        private readonly IAuthService _authService;
        private readonly JwtOptions _jwtOptions;

        public AuthController(IAuthService authService, IOptions<JwtOptions> jwtOptions)
        {
            _authService = authService;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] RegisterStep1RequestDTO registrationRequest, CancellationToken cancellationToken)
        {
            var registrationResult = await _authService.RegisterAsync(registrationRequest, cancellationToken);
            
            return Ok(registrationResult);
        }

        [HttpPost("complete-registration")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationRequestDTO completionRequest, CancellationToken cancellationToken)
        {
            var authResult = await _authService.CompleteRegistrationAsync(completionRequest, cancellationToken);
            AppendRefreshCookie(authResult.RefreshToken);
            
            return Ok(authResult);
        }

        [HttpPost("resend-verification-code")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationCodeRequestDTO resendRequest, CancellationToken cancellationToken)
        {
            await _authService.ResendVerificationCodeAsync(resendRequest, cancellationToken);
            
            return NoContent();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest, CancellationToken cancellationToken)
        {
            var authResult = await _authService.LoginAsync(loginRequest, cancellationToken);
            AppendRefreshCookie(authResult.RefreshToken);
            
            return Ok(authResult);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            
            if (string.IsNullOrWhiteSpace(refreshToken)) 
            {
                throw new UnauthorizedAccessException("Refresh token is missing.");
            }

            var authResult = await _authService.RefreshAsync(refreshToken, cancellationToken);
            AppendRefreshCookie(authResult.RefreshToken);
            
            return Ok(authResult);
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _authService.LogoutAsync(refreshToken, cancellationToken);
                Response.Cookies.Delete("refreshToken", BuildCookieOptions(DateTimeOffset.UtcNow.AddDays(-1)));
            }

            return NoContent();
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO changePasswordRequest, CancellationToken cancellationToken)
        {
            var userId = GetUserIdOrThrow();
            await _authService.ChangePasswordAsync(userId, changePasswordRequest, cancellationToken);
            
            return NoContent();
        }

        private void AppendRefreshCookie(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }
            
            var cookieOptions = BuildCookieOptions(DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays));
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private static CookieOptions BuildCookieOptions(DateTimeOffset expiresAt) => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAt,
            Path = "/"
        };

        private Guid GetUserIdOrThrow()
        {
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
            
            if (!Guid.TryParse(nameIdentifier, out var userId)) 
            {
                throw new UnauthorizedAccessException();
            }
            
            return userId;
        }
    }
}
