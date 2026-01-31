using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ImagesShop.Application.DTOs.Auth;
using ImagesShop.Application.Helpers;
using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using ImagesShop.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ImagesShop.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly JwtOptions _jwtOptions;

        public AuthService(IUserRepository users, IRefreshTokenRepository refreshTokens, IOptions<JwtOptions> jwtOptions)
        {
            _users = users;
            _refreshTokens = refreshTokens;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request, CancellationToken cancellationToken = default)
        {
            var existing = await _users.GetByEmailAsync(request.Email, cancellationToken);
            if (existing is not null) throw new InvalidOperationException("User already exists");

            var (hash, salt) = PasswordHasher.HashPassword(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Balance = 0m,
                Role = UserRole.User
            };

            await _users.AddAsync(user, cancellationToken);

            return await IssueTokensAsync(user, cancellationToken);
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null) throw new InvalidOperationException("Invalid credentials");

            var valid = PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!valid) throw new InvalidOperationException("Invalid credentials");

            return await IssueTokensAsync(user, cancellationToken);
        }

        public async Task<AuthResponseDTO> RefreshAsync(RefreshRequestDTO request, CancellationToken cancellationToken = default)
        {
            var stored = await _refreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (stored is null || !stored.IsActive) throw new InvalidOperationException("Invalid refresh token");

            await _refreshTokens.RevokeAsync(stored, cancellationToken);

            return await IssueTokensAsync(stored.User, cancellationToken);
        }

        private async Task<AuthResponseDTO> IssueTokensAsync(User user, CancellationToken cancellationToken)
        {
            var accessToken = GenerateJwt(user, out var expires);
            var refresh = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = Guid.NewGuid().ToString("N"),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokens.AddAsync(refresh, cancellationToken);

            return new AuthResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refresh.Token,
                ExpiresAtUtc = expires,
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role
            };
        }

        private string GenerateJwt(User user, out DateTime expires)
        {
            expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
