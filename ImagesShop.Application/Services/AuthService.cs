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

namespace ImagesShop.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IEmailSender _emailSender;
        private readonly JwtOptions _jwtOptions;
        private readonly EmailVerificationOptions _emailVerificationOptions;
        private readonly IEmailVerificationCodeRepository _emailVerificationCodeRepository;
        private readonly IPendingRegistrationRepository _pendingRegistrationRepository;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IEmailSender emailSender,
            IOptions<JwtOptions> jwtOptions,
            IOptions<EmailVerificationOptions> emailVerificationOptions,
            IEmailVerificationCodeRepository emailVerificationCodeRepository,
            IPendingRegistrationRepository pendingRegistrationRepository)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _emailSender = emailSender;
            _jwtOptions = jwtOptions.Value;
            _emailVerificationOptions = emailVerificationOptions.Value;
            _emailVerificationCodeRepository = emailVerificationCodeRepository;
            _pendingRegistrationRepository = pendingRegistrationRepository;
        }

        public async Task<RegisterInitResponseDTO> RegisterAsync(RegisterStep1RequestDTO registrationRequest, CancellationToken cancellationToken = default)
        {
            if (registrationRequest is null) throw new ArgumentNullException(nameof(registrationRequest));
            if (string.IsNullOrWhiteSpace(registrationRequest.Email)) throw new InvalidOperationException("Email is required.");
            if (string.IsNullOrWhiteSpace(registrationRequest.Password)) throw new InvalidOperationException("Password is required.");
            
            if (!string.Equals(registrationRequest.Password, registrationRequest.ConfirmPassword, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Passwords do not match.");
            }

            // normalize email
            registrationRequest.Email = registrationRequest.Email.Trim().ToLowerInvariant();

            var existingUser = await _userRepository.GetByEmailAsync(registrationRequest.Email, cancellationToken);
            if (existingUser is not null) 
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            var (passwordHash, passwordSalt) = PasswordHasher.HashPassword(registrationRequest.Password);

            var codeTimeToLive = TimeSpan.FromMinutes(_emailVerificationOptions.CodeTtlMinutes);
            var pendingRegistration = new PendingRegistration
            {
                Id = Guid.NewGuid(),
                Name = registrationRequest.Name,
                Email = registrationRequest.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(codeTimeToLive)
            };

            await _pendingRegistrationRepository.UpsertAsync(pendingRegistration, cancellationToken);

            await CreateAndSendVerificationCodeAsync(pendingRegistration.Email, cancellationToken);

            return new RegisterInitResponseDTO
            {
                Email = pendingRegistration.Email,
                VerificationRequired = true
            };
        }

        public async Task<AuthResponseDTO> CompleteRegistrationAsync(CompleteRegistrationRequestDTO completionRequest, CancellationToken cancellationToken = default)
        {
            if (completionRequest is null) throw new ArgumentNullException(nameof(completionRequest));
            if (string.IsNullOrWhiteSpace(completionRequest.Email)) throw new InvalidOperationException("Email is required.");
            if (string.IsNullOrWhiteSpace(completionRequest.Code)) throw new InvalidOperationException("Verification code is required.");

            // normalize inputs
            completionRequest.Code = completionRequest.Code.Trim();
            completionRequest.Email = completionRequest.Email.Trim().ToLowerInvariant();

            var hasPasswordProvided = !string.IsNullOrWhiteSpace(completionRequest.Password) || !string.IsNullOrWhiteSpace(completionRequest.ConfirmPassword);
            if (hasPasswordProvided)
            {
                if (string.IsNullOrWhiteSpace(completionRequest.Password)) throw new InvalidOperationException("Password is required.");
                if (!string.Equals(completionRequest.Password, completionRequest.ConfirmPassword, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Passwords do not match.");
                }
            }

            var existingUser = await _userRepository.GetByEmailAsync(completionRequest.Email, cancellationToken);
            if (existingUser is not null) throw new InvalidOperationException("User already exists.");

            var pendingRegistration = await _pendingRegistrationRepository.GetByEmailAsync(completionRequest.Email, cancellationToken);
            if (pendingRegistration is null || pendingRegistration.IsExpired) 
            {
                throw new InvalidOperationException("Registration has expired. Please start the registration process again.");
            }

            if (!string.Equals(pendingRegistration.Name ?? string.Empty, completionRequest.Name ?? string.Empty, StringComparison.Ordinal))
            {
                pendingRegistration.Name = completionRequest.Name;
            }

            if (hasPasswordProvided)
            {
                var isPasswordValid = PasswordHasher.Verify(completionRequest.Password, pendingRegistration.PasswordHash, pendingRegistration.PasswordSalt);
                if (!isPasswordValid)
                {
                    throw new InvalidOperationException("The password does not match the pending registration.");
                }
            }

            if (completionRequest.Code.Length != _emailVerificationOptions.CodeLength)
            {
                throw new InvalidOperationException("Invalid code length.");
            }

            var latestActiveCode = await _emailVerificationCodeRepository.GetLatestActiveByEmailAsync(completionRequest.Email, cancellationToken);
            if (latestActiveCode is null)
            {
                throw new InvalidOperationException("The verification code is invalid or has expired.");
            }

            var isCodeValid = PasswordHasher.Verify(completionRequest.Code, latestActiveCode.CodeHash, latestActiveCode.CodeSalt, iterations: 10_000, hashSize: 32);
            if (!isCodeValid) 
            {
                throw new InvalidOperationException("The verification code is invalid.");
            }

            await _emailVerificationCodeRepository.MarkUsedAsync(latestActiveCode, cancellationToken);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = pendingRegistration.Name,
                Email = pendingRegistration.Email,
                PasswordHash = pendingRegistration.PasswordHash,
                PasswordSalt = pendingRegistration.PasswordSalt,
                Balance = 0m,
                Role = UserRole.User,
                EmailVerified = true,
                EmailVerifiedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(newUser, cancellationToken);
            await _pendingRegistrationRepository.DeleteAsync(pendingRegistration, cancellationToken);

            return await IssueTokensAsync(newUser, cancellationToken);
        }

        public async Task ResendVerificationCodeAsync(ResendVerificationCodeRequestDTO resendRequest, CancellationToken cancellationToken = default)
        {
            if (resendRequest is null) throw new ArgumentNullException(nameof(resendRequest));
            if (string.IsNullOrWhiteSpace(resendRequest.Email)) throw new InvalidOperationException("Email is required.");

            resendRequest.Email = resendRequest.Email.Trim().ToLowerInvariant();

            var existingUser = await _userRepository.GetByEmailAsync(resendRequest.Email, cancellationToken);
            if (existingUser is not null && existingUser.EmailVerified) 
            {
                return;
            }

            var pendingRegistration = await _pendingRegistrationRepository.GetByEmailAsync(resendRequest.Email, cancellationToken);
            if (pendingRegistration is null || pendingRegistration.IsExpired) 
            {
                return;
            }

            var lastVerificationCode = await _emailVerificationCodeRepository.GetLatestByEmailAsync(resendRequest.Email, cancellationToken);
            if (lastVerificationCode is not null)
            {
                var resendCooldown = TimeSpan.FromSeconds(_emailVerificationOptions.ResendCooldownSeconds);
                if (DateTime.UtcNow - lastVerificationCode.CreatedAt < resendCooldown)
                {
                    throw new InvalidOperationException("Please wait a moment before requesting another verification code.");
                }
            }

            await CreateAndSendVerificationCodeAsync(resendRequest.Email, cancellationToken);
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO loginRequest, CancellationToken cancellationToken = default)
        {
            var userEntity = await _userRepository.GetByEmailAsync(loginRequest.Email, cancellationToken);
            if (userEntity is null) 
            {
                throw new InvalidOperationException("Invalid login credentials.");
            }

            var isPasswordValid = PasswordHasher.Verify(loginRequest.Password, userEntity.PasswordHash, userEntity.PasswordSalt);
            if (!isPasswordValid) 
            {
                throw new InvalidOperationException("Invalid login credentials.");
            }

            if (!userEntity.EmailVerified) 
            {
                throw new InvalidOperationException("Your email address has not been verified.");
            }

            return await IssueTokensAsync(userEntity, cancellationToken);
        }

        public async Task<AuthResponseDTO> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) 
            {
                throw new InvalidOperationException("The refresh token is invalid.");
            }

            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
            if (storedToken is null || !storedToken.IsActive) 
            {
                throw new InvalidOperationException("The refresh token is invalid or has expired.");
            }

            var authResponse = await IssueTokensAsync(storedToken.User, cancellationToken);

            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.ReplacedByToken = authResponse.RefreshToken;
            await _refreshTokenRepository.RevokeAsync(storedToken, cancellationToken);

            return authResponse;
        }

        public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) 
            {
                return;
            }

            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
            if (storedToken is null || storedToken.RevokedAt is not null) 
            {
                return;
            }

            storedToken.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.RevokeAsync(storedToken, cancellationToken);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDTO changePasswordRequest, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) throw new InvalidOperationException("Invalid user identifier.");
            if (changePasswordRequest is null) throw new ArgumentNullException(nameof(changePasswordRequest));
            if (string.IsNullOrWhiteSpace(changePasswordRequest.CurrentPassword)) throw new InvalidOperationException("Current password is required.");
            if (string.IsNullOrWhiteSpace(changePasswordRequest.NewPassword)) throw new InvalidOperationException("New password is required.");
            
            if (!string.Equals(changePasswordRequest.NewPassword, changePasswordRequest.ConfirmPassword, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Passwords do not match.");
            }

            var userEntity = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (userEntity is null) throw new InvalidOperationException("User not found.");

            var isCurrentPasswordValid = PasswordHasher.Verify(changePasswordRequest.CurrentPassword, userEntity.PasswordHash, userEntity.PasswordSalt);
            if (!isCurrentPasswordValid) 
            {
                throw new InvalidOperationException("The current password provided is incorrect.");
            }

            var (newHash, newSalt) = PasswordHasher.HashPassword(changePasswordRequest.NewPassword);
            userEntity.PasswordHash = newHash;
            userEntity.PasswordSalt = newSalt;

            await _userRepository.UpdateAsync(userEntity, cancellationToken);
        }

        private async Task CreateAndSendVerificationCodeAsync(string email, CancellationToken cancellationToken)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            var numericVerificationCode = GenerateNumericCode(_emailVerificationOptions.CodeLength);
            var (codeHash, codeSalt) = PasswordHasher.HashPassword(numericVerificationCode, iterations: 10_000);

            var verificationCodeEntity = new EmailVerificationCode
            {
                Id = Guid.NewGuid(),
                UserId = null,
                Email = normalizedEmail,
                CodeHash = codeHash,
                CodeSalt = codeSalt,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_emailVerificationOptions.CodeTtlMinutes)
            };

            await _emailVerificationCodeRepository.AddForEmailAsync(normalizedEmail, verificationCodeEntity, cancellationToken);

            await _emailSender.SendEmailAsync(
                normalizedEmail,
                "Email verification code",
                $"Your verification code is: {numericVerificationCode}",
                cancellationToken);
        }

        private static string GenerateNumericCode(int length)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(length);
            var Characters = new char[length];
            for (var index = 0; index < length; index++)
            {
                Characters[index] = (char)('0' + (randomBytes[index] % 10));
            }
            return new string(Characters);
        }

        private async Task<AuthResponseDTO> IssueTokensAsync(User userEntity, CancellationToken cancellationToken)
        {
            var accessToken = GenerateJwt(userEntity, out var expirationTime);
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = Guid.NewGuid().ToString("N"),
                UserId = userEntity.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

            return new AuthResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenEntity.Token,
                ExpiresAtUtc = expirationTime,
                UserId = userEntity.Id,
                Email = userEntity.Email,
                Name = userEntity.Name,
                Role = userEntity.Role
            };
        }

        private string GenerateJwt(User userEntity, out DateTime expirationTime)
        {
            expirationTime = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
            var userClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userEntity.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, userEntity.Email),
                new(ClaimTypes.Name, userEntity.Name),
                new(ClaimTypes.Role, userEntity.Role.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: userClaims,
                expires: expirationTime,
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }
    }
}
