using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UpgradeOctoSystem.Abstractions.Models;
using UpgradeOctoSystem.Abstractions.Models.Auth;
using UpgradeOctoSystem.Abstractions.Services;
using UpgradeOctoSystem.Api.Models;
using UpgradeOctoSystem.Database;
using UpgradeOctoSystem.Database.Models;

namespace UpgradeOctoSystem.Api.Services
{
    public class AuthService : ITokenProvider, IUserProvider, IForgotPasswordProvider
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ClaimsPrincipal _userClaim;
        private readonly DatabaseContext _context;
        private readonly IMailerService _mailer;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ClaimsPrincipal userClaim,
            IConfiguration configuration,
            DatabaseContext husenseDatabaseContext,
            IMailerService mailer)
        {
            _userManager = userManager;
            _userClaim = userClaim;
            _configuration = configuration;
            _context = husenseDatabaseContext;
            _mailer = mailer;
        }

        public async Task<string> GenerateAccessTokenAsync(IdentityUser user)
        {
            var AuthClaims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:Issuer"],
                audience: _configuration["AuthSettings:Audience"],
                claims: AuthClaims,
                expires: DateTimeOffset.UtcNow.AddDays(1).DateTime,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task RegisterUserAsync(IRegisterRequest model)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted))
            {
                try
                {
                    var user = new ApplicationUser
                    {
                        Email = model.Email,
                        UserName = model.Email,
                        PhoneNumber = model.PhoneNumber,
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (!result.Succeeded) // TODO: result.Errors to error list
                        throw new ValidationException(ExceptionMapping.Register_OrganizationExists);

                    var refreshToken = await _userManager.GenerateConcurrencyStampAsync(user);
                    var refreshTokenResult = await _context.UserRefreshTokens.AddAsync(new UserRefreshToken
                    {
                        UserID = user.Id,
                        RefreshToken = refreshToken,
                        ExpiryDate = DateTime.Now.AddDays(1)
                    });

                    await _userManager.UpdateAsync(user);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _mailer.SendEmailVerificationAsync(user.Email, $"{user.UserName}", confirmationToken, Guid.Parse(user.Id));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new ValidationException("Failed to create user", ex);
                }
            }
        }

        public async Task<IAuthResponse> LoginUserAsync(ILoginRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
            if (user == null)
                throw new ValidationException(ExceptionMapping.None, "Combination of user/password does not exist");

            if (!await _userManager.IsEmailConfirmedAsync(user))
                throw new ValidationException(ExceptionMapping.None, "Email not confirmed");

            var result = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!result)
                throw new ValidationException(ExceptionMapping.None, "Combination of user/password does not exist");

            var accessToken = await GenerateAccessTokenAsync(user).ConfigureAwait(false);
            var refreshToken = await _userManager.GenerateConcurrencyStampAsync(user).ConfigureAwait(false);

            var userRefreshToken = await _context.UserRefreshTokens.AsTracking().FirstOrDefaultAsync(x => x.UserID == user.Id);
            if (userRefreshToken == null)
                throw new ValidationException(ExceptionMapping.None, "User could not be created");

            if (DateTimeOffset.UtcNow <= userRefreshToken.ExpiryDate)
            {
                refreshToken = userRefreshToken.RefreshToken;
            }
            else
            {
                userRefreshToken.ExpiryDate = DateTimeOffset.UtcNow.AddDays(1);
                userRefreshToken.RefreshToken = refreshToken;
                userRefreshToken.UserID = user.Id;
                _context.UserRefreshTokens.Update(userRefreshToken);
                await _context.SaveChangesAsync();
            }

            return new AuthResponse
            {
                AccessToken = accessToken,
                ExpireDate = DateTimeOffset.UtcNow.AddDays(1).DateTime,
                RefreshToken = refreshToken
            };
        }

        public async Task ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ValidationException(ExceptionMapping.None, "User not found");

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                throw new ValidationException(ExceptionMapping.None, "Combination of user/password does not exist");
        }

        public async Task ForgetPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new ValidationException(ExceptionMapping.None, "Combination of user/password does not exist");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _mailer.SendForgotPasswordMailAsync(user.Email, $"{user.UserName}", token, Guid.Parse(user.Id));
        }

        public async Task ResetPasswordAsync(IResetPasswordRequest model)
        {
            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null)
                throw new ValidationException(ExceptionMapping.None, "does not exist");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (!result.Succeeded)
                throw new ValidationException(ExceptionMapping.None, "failed");
        }

        public async Task<IAuthResponse> RefreshTokenAsync(IAuthResponse model)
        {
            var user = await GetUserFromAccessTokenAsync(model.AccessToken);
            if (user != null && (await ValidateRefreshTokenAsync(user, model.RefreshToken)))
            {
                string tokenAsString = await GenerateAccessTokenAsync(user);

                return new AuthResponse
                {
                    AccessToken = tokenAsString,
                    ExpireDate = DateTime.Now.AddDays(1),
                    RefreshToken = model.RefreshToken
                };
            }
            throw new ValidationException(ExceptionMapping.None, "Failed to refresh");
        }

        private async Task<IdentityUser> GetUserFromAccessTokenAsync(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                var principle = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken != null && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    var userId = principle.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    return await _userManager.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
                }
            }
            catch (Exception)
            {
                return new IdentityUser();
            }

            return new IdentityUser();
        }

        private async Task<bool> ValidateRefreshTokenAsync(IdentityUser user, string refreshToken)
        {
            var userRefreshToken = await _context.UserRefreshTokens.AsQueryable().FirstOrDefaultAsync(x => x.RefreshToken == refreshToken && x.UserID == user.Id);

            if (userRefreshToken != null && userRefreshToken.ExpiryDate > DateTime.UtcNow)
                return true;
            return false;
        }
    }
}