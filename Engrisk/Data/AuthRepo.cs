using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Application.Services;
using Infrastructure.Mail;
using Application.DTOs.Auth;
using Application.DTOs.Account;

namespace Engrisk.Data
{
    public class AuthRepo : IAuthRepo
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Account> _userManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public AuthRepo(ApplicationDbContext db, UserManager<Account> userManager, IConfiguration config, IMapper mapper)
        {
            _userManager = userManager;
            _db = db;
            _config = config;
            _mapper = mapper;
        }

        public async Task<Account> AccountAccountForBlog(int id)
        {
            var accountFromDb = await _db.Accounts.Where(acc => acc.Id == id).Include(s => s.Sections).Include(q => q.Histories).Include(e => e.ExamHistories).Include(w => w.Learned).FirstOrDefaultAsync();
            return accountFromDb;
        }

        public async Task<bool> ChangePassword(int id, string currenPass, string newPassword)
        {
            var accountFromDb = await _userManager.FindByIdAsync(id.ToString());
            var result = await _userManager.ChangePasswordAsync(accountFromDb, currenPass, newPassword);
            if (result.Succeeded)
            {
                return true;
            }
            return false;
        }

        public async Task<Account> CreateAccount(Account account)
        {
            await _userManager.CreateAsync(account);
            return account;
        }

        public async Task DeleteAccount(int id)
        {
            var accountFromDb = _db.Accounts.FirstOrDefault(u => u.Id == id);
            await _userManager.DeleteAsync(accountFromDb);
        }

        public async Task DeleteAccount(string username)
        {
            var accountFromDb = _db.Accounts.FirstOrDefault(u => u.UserName.Equals(username));
            await _userManager.DeleteAsync(accountFromDb);
        }

        public bool Exists(string identify)
        {
            return _db.Accounts.Any(u => u.Email.Equals(identify) || u.UserName.Equals(identify) || u.PhoneNumber.Equals(identify));
        }

        public async Task<bool> ForgetPassword(string email)
        {
            try
            {
                int otp = 0;
                int timeRemain = 0;
                Random rand = new Random();
                var accountFromDb = await _db.Accounts.Where(account => account.Email.Equals(email)).Include(i => i.AccountOTP).FirstOrDefaultAsync();
                if (accountFromDb == null)
                {
                    return false;
                }
                var otpAlive = accountFromDb.AccountOTP.Where(acc => acc.AccountId == accountFromDb.Id && acc.IsAlive).FirstOrDefault();
                if (otpAlive == null)
                {
                    otp = rand.Next(10000, 99999);
                    string token = await _userManager.GeneratePasswordResetTokenAsync(accountFromDb);
                    var accountOtp = new AccountOTP()
                    {
                        AccountId = accountFromDb.Id,
                        OTP = otp,
                        ValidUntil = DateTime.Now.AddMinutes(15),
                        Token = token
                    };
                    while (await _db.AccountOTP.AnyAsync(o => o.OTP == otp && o.ValidUntil < DateTime.Now))
                    {
                        otp = rand.Next(10000, 99999);
                    }

                    _db.AccountOTP.Add(accountOtp);
                    timeRemain = 15;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    otp = otpAlive.OTP;
                    timeRemain = otpAlive.ValidUntil.Subtract(DateTime.Now).Minutes;
                }
                var body = $"Đây là mã OTP: {otp} vui lòng dùng nó để reset mật khẩu trong vòng {timeRemain} phút.";
                MailService.SendMail(email, accountFromDb.Fullname, "Quên mật khẩu", body);
                return true;
            }
            catch (System.Exception e)
            {

                throw e;
            }

        }

        public RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.Now.AddDays(7),
                    Created = DateTime.Now,
                    CreatedByIp = ipAddress
                };
            }
        }
        public async Task<AuthenticateRequestDTO> GenerateToken(Account account, string ipAddress)
        {
            var roles = await _userManager.GetRolesAsync(account);
            var claims = new List<Claim>{
                new Claim(ClaimTypes.NameIdentifier,account.Id.ToString()),
                new Claim(ClaimTypes.Name,account.UserName)
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.GetSection("AppSettings:TokenSecret").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(8),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var secureToken = tokenHandler.CreateToken(tokenDescriptor);

            var token = tokenHandler.WriteToken(secureToken);
            string refreshtoken = "";
            var refreshTokenFromDb = account.RefreshTokens.Where(a => a.IsActive && a.AccountId == account.Id).LastOrDefault();
            if (refreshTokenFromDb == null)
            {
                var refreshToken = GenerateRefreshToken(ipAddress);
                account.RefreshTokens.Add(refreshToken);
                _db.Update(account);
                await _db.SaveChangesAsync();
                refreshtoken = refreshToken.Token;
            }
            else
            {
                refreshtoken = refreshTokenFromDb.Token;
            }

            return new AuthenticateRequestDTO()
            {
                Token = token,
                RefreshToken = refreshtoken
            };
        }

        public async Task<Account> GetAccount(string identify)
        {
            var accountFromDb = await _db.Accounts.FirstOrDefaultAsync(u => u.UserName.Equals(identify) || u.Email.Equals(identify));
            return accountFromDb;
        }

        public async Task<Account> GetAccountDetail(string identify)
        {
            var accountFromDb = await _db.Accounts.Where(u => u.UserName.ToLower().Equals(identify.ToLower()) || u.Email.ToLower().Equals(identify.ToLower())).Include(a => a.Roles).ThenInclude(r => r.Role).AsNoTracking().FirstOrDefaultAsync();
            accountFromDb.Following = await _db.Follows.Where(follow => follow.FollowerId == accountFromDb.Id).Include(inc => inc.Following).AsNoTracking().ToListAsync();
            accountFromDb.Followers = await _db.Follows.Where(follow => follow.FollowingId == accountFromDb.Id).Include(inc => inc.Follower).AsNoTracking().ToListAsync();
            return accountFromDb;
        }

        public async Task<Account> GetAccountDetail(int id)
        {
            var accountFromDb = await _db.Accounts.Include(a => a.Roles).ThenInclude(r => r.Role).FirstOrDefaultAsync(u => u.Id == id);
            accountFromDb.Following = await _db.Follows.Where(follow => follow.FollowerId == id).Include(inc => inc.Following).AsNoTracking().ToListAsync();
            accountFromDb.Followers = await _db.Follows.Where(follow => follow.FollowingId == id).Include(inc => inc.Follower).AsNoTracking().ToListAsync();
            return accountFromDb;
        }

        public async Task<IEnumerable<Account>> GetAll()
        {
            var account = _db.Accounts.Include(u => u.Histories).Include(r => r.Roles).ThenInclude(r => r.Role).AsQueryable();
            return await account.ToListAsync();
        }
        public async Task<AuthenticateRequestDTO> RefreshToken(string token, string ipAddress)
        {
            var user = await _db.Accounts.FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token.Equals(token)));
            if (user == null)
            {
                return null;
            }
            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
            if (!refreshToken.IsActive) return null;
            var newRefreshToken = GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            // generate new jwt
            var jwtToken = await GenerateToken(user, ipAddress);
            return new AuthenticateRequestDTO()
            {
                Token = jwtToken.Token,
                RefreshToken = jwtToken.RefreshToken
            };
        }

        public async Task<bool> ResetPassword(Account account, string token, string newPassword)
        {
            var result = await _userManager.ResetPasswordAsync(account, token, newPassword);
            if (result.Succeeded)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> RevokeToken(string token, string ipAddress)
        {
            var user = await _db.Accounts.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            // return false if no user found with token
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _db.Update(user);
            _db.SaveChanges();
            return true;
        }

        public async Task<bool> SaveAll()
        {
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAccount(int id, AccountForUpdateDTO accountForUpdateDTO)
        {
            var accountFromDb = await _userManager.FindByIdAsync(id.ToString());
            if (accountFromDb == null)
            {
                return false;
            }
            _mapper.Map(accountForUpdateDTO, accountFromDb);
            if (await _db.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> VerifyEmail(string email)
        {
            try
            {
                int otp = 0;
                int timeRemain = 0;
                Random rand = new Random();
                var accountFromDb = await _db.Accounts.Where(account => account.Email.Equals(email)).Include(i => i.AccountOTP).FirstOrDefaultAsync();
                if (accountFromDb == null)
                {
                    return false;
                }
                var otpAlive = accountFromDb.AccountOTP.Where(acc => acc.AccountId == accountFromDb.Id && acc.IsAlive).FirstOrDefault();
                if (otpAlive == null)
                {
                    otp = rand.Next(10000, 99999);
                    string token = await _userManager.GenerateEmailConfirmationTokenAsync(accountFromDb);
                    var accountOtp = new AccountOTP()
                    {
                        AccountId = accountFromDb.Id,
                        OTP = otp,
                        ValidUntil = DateTime.Now.AddMinutes(15),
                        Token = token
                    };
                    while (await _db.AccountOTP.AnyAsync(o => o.OTP == otp && o.ValidUntil < DateTime.Now))
                    {
                        otp = rand.Next(10000, 99999);
                    }

                    _db.AccountOTP.Add(accountOtp);
                    timeRemain = 15;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    otp = otpAlive.OTP;
                    timeRemain = otpAlive.ValidUntil.Subtract(DateTime.Now).Minutes;
                }
                var body = $"Đây là mã OTP: {otp} vui lòng dùng nó để kích hoạt email trong vòng {timeRemain} phút.";
                MailService.SendMail(email, accountFromDb.Fullname, "Xác nhận email", body);
                return true;
            }
            catch (System.Exception e)
            {

                throw e;
            }
        }
    }
}