using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Account;
using Application.DTOs.Auth;
using Domain.Models;

namespace Engrisk.Data
{
    public interface IAuthRepo
    {
        Task<IEnumerable<Account>> GetAll();
        Task<Account> GetAccountDetail(string identify);
        Task<Account> GetAccountDetail(int id);
        Task<Account> AccountAccountForBlog(int id);
        Task<Account> GetAccount(string identity);
        Task<Account> CreateAccount(Account account);
        Task<bool> UpdateAccount(int id, AccountForUpdateDTO accountForUpdateDTO);
        Task<bool> ChangePassword(int id,string currentPass,string newPassword);
        Task<bool> ForgetPassword(string email);
        Task<bool> ResetPassword(Account account,string token, string newPassword);
        Task<bool> VerifyEmail(string email);
        Task DeleteAccount(int id);
        Task DeleteAccount(string username);
        Task<bool> SaveAll();
        Task<AuthenticateRequestDTO> GenerateToken(Account account, string ipAddress);
        RefreshToken GenerateRefreshToken(string ipAddress);
        Task<bool> RevokeToken(string token, string ipAddress);
        Task<AuthenticateRequestDTO> RefreshToken(string token, string ipAddress);
        bool Exists(string identify);
    }
}