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
using System.Security.Cryptography;
using System.Linq;

namespace ImagesShop.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly IEmailSender _email;
        private readonly JwtOptions _jwtOptions;
        private readonly EmailVerificationOptions _emailVerificationOptions;
        private readonly IEmailVerificationCodeRepository _emailVerificationCodes;
        private readonly IPendingRegistrationRepository _pendingRegistrations;

        public AuthService(
            IUserRepository users,
            IRefreshTokenRepository refreshTokens,
            IEmailSender email,
            IOptions<JwtOptions> jwtOptions,
            IOptions<EmailVerificationOptions> emailVerificationOptions,
            IEmailVerificationCodeRepository emailVerificationCodes,
            IPendingRegistrationRepository pendingRegistrations)
        {
            _users = users;
            _refreshTokens = refreshTokens;
            _email = email;
            _jwtOptions = jwtOptions.Value;
            _emailVerificationOptions = emailVerificationOptions.Value;
            _emailVerificationCodes = emailVerificationCodes;
            _pendingRegistrations = pendingRegistrations;
        }

        public async Task<RegisterInitResponseDTO> RegisterAsync(RegisterStep1RequestDTO request, CancellationToken cancellationToken = default)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Email)) throw new InvalidOperationException("Email is required");
            if (string.IsNullOrWhiteSpace(request.Password)) throw new InvalidOperationException("Password is required");
            if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
                throw new InvalidOperationException("Passwords do not match");

            // normalize email
            request.Email = request.Email.Trim().ToLowerInvariant();

            var existingUser = await _users.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser is not null) throw new InvalidOperationException("User already exists");

            var (hash, salt) = PasswordHasher.HashPassword(request.Password);

            var ttl = TimeSpan.FromMinutes(_emailVerificationOptions.CodeTtlMinutes);
            var pending = new PendingRegistration
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(ttl)
            };

            await _pendingRegistrations.UpsertAsync(pending, cancellationToken);

            await CreateAndSendVerificationCodeAsync(pending.Email, cancellationToken);

            return new RegisterInitResponseDTO
            {
                Email = pending.Email,
                VerificationRequired = true
            };
        }

        public async Task<AuthResponseDTO> CompleteRegistrationAsync(CompleteRegistrationRequestDTO request, CancellationToken cancellationToken = default)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Email)) throw new InvalidOperationException("Email is required");
            if (string.IsNullOrWhiteSpace(request.Code)) throw new InvalidOperationException("Code is required");

            // normalize inputs
            request.Code = request.Code.Trim();
            request.Email = request.Email.Trim().ToLowerInvariant();

            Console.WriteLine($"[DEBUG] CompleteRegistration request: Email='{request.Email}', Code='{request.Code}', CodeLength={request.Code.Length}");

            var hasPassword = !string.IsNullOrWhiteSpace(request.Password) || !string.IsNullOrWhiteSpace(request.ConfirmPassword);
            if (hasPassword)
            {
                if (string.IsNullOrWhiteSpace(request.Password)) throw new InvalidOperationException("Password is required");
                if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
                    throw new InvalidOperationException("Passwords do not match");
            }

            var existingUser = await _users.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser is not null) throw new InvalidOperationException("User already exists");

            var pending = await _pendingRegistrations.GetByEmailAsync(request.Email, cancellationToken);
            if (pending is null || pending.IsExpired) throw new InvalidOperationException("Registration expired. Please register again.");

            if (!string.Equals(pending.Name ?? string.Empty, request.Name ?? string.Empty, StringComparison.Ordinal))
            {
                pending.Name = request.Name;
            }

            if (hasPassword)
            {
                var passOk = PasswordHasher.Verify(request.Password, pending.PasswordHash, pending.PasswordSalt);
                if (!passOk) throw new InvalidOperationException("Password does not match pending registration");
            }

            if (request.Code.Length != _emailVerificationOptions.CodeLength)
                throw new InvalidOperationException("Invalid code length");

            var latest = await _emailVerificationCodes.GetLatestActiveByEmailAsync(request.Email, cancellationToken);
            if (latest is null)
            {
                var latestAny = await _emailVerificationCodes.GetLatestByEmailAsync(request.Email, cancellationToken);
                if (latestAny is null)
                {
                    Console.WriteLine($"[DEBUG] No verification codes found for {request.Email}");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] Latest (any) code for {request.Email}: CreatedAt={latestAny.CreatedAt:o}, ExpiresAt={latestAny.ExpiresAt:o}, UsedAt={(latestAny.UsedAt.HasValue ? latestAny.UsedAt.Value.ToString("o") : "null")}");
                }

                throw new InvalidOperationException("Invalid code or code expired");
            }

            // Debug output to help diagnose verification failures
            Console.WriteLine($"[DEBUG] Latest active code for {request.Email}: CreatedAt={latest.CreatedAt:o}, ExpiresAt={latest.ExpiresAt:o}, UsedAt={(latest.UsedAt.HasValue ? latest.UsedAt.Value.ToString("o") : "null")}");
            Console.WriteLine($"[DEBUG] Stored CodeHash (base64): {latest.CodeHash}");
            Console.WriteLine($"[DEBUG] Stored CodeSalt (base64): {latest.CodeSalt}");
            try
            {
                var saltBytes = Convert.FromBase64String(latest.CodeSalt);
                using var pbkdf2 = new Rfc2898DeriveBytes(request.Code, saltBytes, 10_000, HashAlgorithmName.SHA256);
                var computed = pbkdf2.GetBytes(32);
                Console.WriteLine($"[DEBUG] Computed hash (base64) using provided code: {Convert.ToBase64String(computed)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Failed to compute hash for debug: {ex}");
            }

            var codeOk = PasswordHasher.Verify(request.Code, latest.CodeHash, latest.CodeSalt, iterations: 10_000, hashSize: 32);
            if (!codeOk) throw new InvalidOperationException("Invalid code");

            await _emailVerificationCodes.MarkUsedAsync(latest, cancellationToken);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = pending.Name,
                Email = pending.Email,
                PasswordHash = pending.PasswordHash,
                PasswordSalt = pending.PasswordSalt,
                Balance = 0m,
                Role = UserRole.User,
                EmailVerified = true,
                EmailVerifiedAt = DateTime.UtcNow
            };

            await _users.AddAsync(user, cancellationToken);
            await _pendingRegistrations.DeleteAsync(pending, cancellationToken);

            return await IssueTokensAsync(user, cancellationToken);
        }

        public async Task ResendVerificationCodeAsync(ResendVerificationCodeRequestDTO request, CancellationToken cancellationToken = default)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Email)) throw new InvalidOperationException("Email is required");

            request.Email = request.Email.Trim().ToLowerInvariant();

            var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
            if (user is not null && user.EmailVerified) return;

            var pending = await _pendingRegistrations.GetByEmailAsync(request.Email, cancellationToken);
            if (pending is null || pending.IsExpired) return;

            var last = await _emailVerificationCodes.GetLatestByEmailAsync(request.Email, cancellationToken);
            if (last is not null)
            {
                var cooldown = TimeSpan.FromSeconds(_emailVerificationOptions.ResendCooldownSeconds);
                if (DateTime.UtcNow - last.CreatedAt < cooldown)
                    throw new InvalidOperationException("Please wait before requesting another code");
            }

            await CreateAndSendVerificationCodeAsync(request.Email, cancellationToken);
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null) throw new InvalidOperationException("Invalid credentials");

            var valid = PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!valid) throw new InvalidOperationException("Invalid credentials");

            if (!user.EmailVerified) throw new InvalidOperationException("Email is not verified");

            return await IssueTokensAsync(user, cancellationToken);
        }

        public async Task<AuthResponseDTO> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) throw new InvalidOperationException("Invalid refresh token");

            var stored = await _refreshTokens.GetByTokenAsync(refreshToken, cancellationToken);
            if (stored is null || !stored.IsActive) throw new InvalidOperationException("Invalid refresh token");

            var result = await IssueTokensAsync(stored.User, cancellationToken);

            stored.RevokedAt = DateTime.UtcNow;
            stored.ReplacedByToken = result.RefreshToken;
            await _refreshTokens.RevokeAsync(stored, cancellationToken);

            return result;
        }

        public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return;

            var stored = await _refreshTokens.GetByTokenAsync(refreshToken, cancellationToken);
            if (stored is null || stored.RevokedAt is not null) return;

            stored.RevokedAt = DateTime.UtcNow;
            await _refreshTokens.RevokeAsync(stored, cancellationToken);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDTO request, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) throw new InvalidOperationException("Invalid user");
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.CurrentPassword)) throw new InvalidOperationException("Current password is required");
            if (string.IsNullOrWhiteSpace(request.NewPassword)) throw new InvalidOperationException("New password is required");
            if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
                throw new InvalidOperationException("Passwords do not match");

            var user = await _users.GetByIdAsync(userId, cancellationToken);
            if (user is null) throw new InvalidOperationException("User not found");

            var valid = PasswordHasher.Verify(request.CurrentPassword, user.PasswordHash, user.PasswordSalt);
            if (!valid) throw new InvalidOperationException("Invalid current password");

            var (hash, salt) = PasswordHasher.HashPassword(request.NewPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;

            await _users.UpdateAsync(user, cancellationToken);
        }

        private async Task CreateAndSendVerificationCodeAsync(string email, CancellationToken cancellationToken)
        {
            email = email.Trim().ToLowerInvariant();

            var code = GenerateNumericCode(_emailVerificationOptions.CodeLength);
            var (hash, salt) = PasswordHasher.HashPassword(code, iterations: 10_000);

            var entity = new EmailVerificationCode
            {
                Id = Guid.NewGuid(),
                UserId = null,
                Email = email,
                CodeHash = hash,
                CodeSalt = salt,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_emailVerificationOptions.CodeTtlMinutes)
            };

            await _emailVerificationCodes.AddForEmailAsync(email, entity, cancellationToken);

            Console.WriteLine($"[DEBUG] Generated verification code for {email}: {code}");
            Console.WriteLine($"[DEBUG] Code metadata: Id={entity.Id}, CreatedAt={entity.CreatedAt:o}, ExpiresAt={entity.ExpiresAt:o}, CodeLength={_emailVerificationOptions.CodeLength}");

            await _email.SendEmailAsync(
                email,
                "Email verification code",
                $"Your verification code is: {code}",
                cancellationToken);
        }

        private static string GenerateNumericCode(int length)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var chars = new char[length];
            for (var i = 0; i < length; i++)
                chars[i] = (char)('0' + (bytes[i] % 10));
            return new string(chars);
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
