using System;
using System.Net.Http;
using System.Threading.Tasks;
using Engrisk.Data;
using Application.Helper;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Persistence;
using Application.DTOs.Account;
using Application.DTOs.Auth;

namespace Engrisk.Services
{
    public class FacebookAuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Account> _userManager;
        private const string AuthURL = "https://graph.facebook.com/debug_token?input_token={0}&access_token={1}|{2}";
        private const string UserInfoUrl = "https://graph.facebook.com/me?fields=id,first_name,last_name,picture,email&access_token={0}";
        private readonly IHttpClientFactory _httpClient;
        private readonly FacebookAuthConfig _facebookConfig;
        private readonly IConfiguration _config;
        private readonly IAuthRepo _authRepo;
        public FacebookAuthService(ApplicationDbContext db, UserManager<Account> userManager, IOptions<FacebookAuthConfig> facebookConfig, IHttpClientFactory httpClient, IConfiguration config, IAuthRepo authRepo)
        {
            _db = db;
            _userManager = userManager;
            _facebookConfig = new FacebookAuthConfig()
            {
                AppId = facebookConfig.Value.AppId,
                AppSecret = facebookConfig.Value.AppSecret
            };
            _httpClient = httpClient;
            _config = config;
            _authRepo = authRepo;
        }

        public async Task<Account> Authenticate(UserLoginRequest request)
        {
            var formattedUrl = string.Format(AuthURL, request.Token, _facebookConfig.AppId, _facebookConfig.AppSecret);
            var result = await _httpClient.CreateClient().GetAsync(formattedUrl);
            result.EnsureSuccessStatusCode();
            var response = await result.Content.ReadAsStringAsync();
            var socialAccount = JsonConvert.DeserializeObject<SocialAccountDTO>(response);
            if (socialAccount.Data.IsValid)
            {
                return await GetInfo(request.Token, socialAccount.Data.UserId);
            }
            return null;
        }

        public async Task<Account> GetInfo(string accessToken, string userId)
        {
            var formattedUrl = string.Format(UserInfoUrl, accessToken);
            var result = await _httpClient.CreateClient().GetAsync(formattedUrl);
            result.EnsureSuccessStatusCode();
            var response = await result.Content.ReadAsStringAsync();
            var socialAccount = JsonConvert.DeserializeObject<SocialAuthDTO>(response);
            return await GetOrCreateExternalLoginUser(UserLoginRequest.FacebookProvider, userId, socialAccount.Email, socialAccount.First_name + " " + socialAccount.Last_name, socialAccount.Picture.Data.Url);
        }
        public async Task<Account> GetOrCreateExternalLoginUser(string provider, string key, string email, string fullname, string picture)
        {
            var user = await _userManager.FindByLoginAsync(provider, key);
            if (user != null)
            {
                var account = await _authRepo.GetAccountDetail(user.Id);
                account.PhotoUrl = picture;
                await _db.SaveChangesAsync();
                return account;
            }
            var userFromEmail = await _userManager.FindByEmailAsync(email);
            if (userFromEmail == null)
            {
                userFromEmail = new Account()
                {
                    UserName = key,
                    Email = email,
                    PhotoUrl = picture,
                    Fullname = fullname,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(userFromEmail, _config.GetSection("Defaults:Account:Password").Value);
                if(result.Succeeded){
                    await _userManager.AddToRoleAsync(userFromEmail, "learner");
                }
            }
            if (string.IsNullOrEmpty(userFromEmail.PhotoUrl))
            {
                userFromEmail.PhotoUrl = picture;
                await _db.SaveChangesAsync();
            }
            var userInfo = new UserLoginInfo(provider, key, provider.ToUpperInvariant());
            var loginResult = await _userManager.AddLoginAsync(userFromEmail, userInfo);
            if (loginResult.Succeeded)
            {
                return userFromEmail;
            }
            return null;
        }
    }
}