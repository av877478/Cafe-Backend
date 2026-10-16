using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cafe.Service
{
    public class AccountService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AccountService(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        #region Register Admin

        /// <summary>
        /// Registers a new Admin user with PBKDF2 hashed password and active status.
        /// </summary>
        public async Task<RegistrationCompletionResponseDto> RegisterAdminAsync(RegistrationRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            // 1. Check if user already exists
            var existingUser = await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (existingUser)
            {
                throw new InvalidOperationException("An account with this email address already exists.");
            }

            // 2. Hash password securely with PBKDF2 + SHA512 + Salt
            var (hash, salt) = HashPassword(request.Password);

            // 3. Create Admin entity
            var adminUser = new User
            {
                FirstName = request.FirstName.Trim(),
                Email = normalizedEmail,
                MobileNumber = request.MobileNumber.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                Address = request.Address.Trim(),
                Role = "Admin",
                AccountStatus = "ACTIVE",
                IsEmailVerified = true,
                PrivacyPolicyAccepted = request.PrivacyPolicyAccepted,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Users.AddAsync(adminUser);
            await _dbContext.SaveChangesAsync();

            return new RegistrationCompletionResponseDto
            {
                RegistrationCompleted = true,
                EmailVerified = true,
                AccountStatus = adminUser.AccountStatus,
                Message = "Your admin account has been created successfully.",
                AdminName = adminUser.FirstName
            };
        }

        #endregion

        #region Authentication (Login & JWT)

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (user.AccountStatus == "SUSPENDED")
            {
                throw new UnauthorizedAccessException("This account has been suspended. Please contact support.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var token = GenerateJwtToken(user, request.RememberMe);

            return new LoginResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.FirstName,
                    MobileNumber = user.MobileNumber,
                    Role = user.Role,
                    AccountStatus = user.AccountStatus
                },
                Message = "Login successful."
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.FirstName,
                MobileNumber = user.MobileNumber,
                Role = user.Role,
                AccountStatus = user.AccountStatus
            };
        }

        #endregion

        #region Helpers & Security

        public string GenerateJwtToken(User user, bool rememberMe)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "zzK45SVMtqn9zNjnuD7YXVdshbdvrtvrxoO6zE2IEm";
            var issuer = _configuration["Jwt:Issuer"] ?? "Cafe";
            var audience = _configuration["Jwt:Audience"] ?? "Cafe Api";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("AccountStatus", user.AccountStatus),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expires = rememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(12);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static (string Hash, string Salt) HashPassword(string password)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
            byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                saltBytes,
                iterations: 100000,
                HashAlgorithmName.SHA512,
                outputLength: 32);

            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        private static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                saltBytes,
                iterations: 100000,
                HashAlgorithmName.SHA512,
                outputLength: 32);

            return CryptographicOperations.FixedTimeEquals(hashBytes, Convert.FromBase64String(storedHash));
        }

        #endregion
    }
}
