using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Application.DTOs;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Persistence;
using Application.DTOs.Account;

namespace Engrisk.Helper
{
    public class Seed
    {
        public static void SeedData(ApplicationDbContext db, UserManager<Account> userManager, RoleManager<Role> roleManager)
        {
            if (!db.DailyMissions.Any())
            {
                var dailyMissionsData = System.IO.File.ReadAllText("Data/DailyMission.json");
                var superAdminAccountData = System.IO.File.ReadAllText("Data/AdminAccount.json");
                var rolesData = System.IO.File.ReadAllText("Data/Role.json");
                var dailyMissions = JsonConvert.DeserializeObject<IEnumerable<DailyMission>>(dailyMissionsData);
                var superAdminAccounts = JsonConvert.DeserializeObject<IEnumerable<AccountForRegisterDTO>>(superAdminAccountData);
                var roles = JsonConvert.DeserializeObject<IEnumerable<Role>>(rolesData);
                foreach (var gift in dailyMissions)
                {
                    if (!db.DailyMissions.Any(dm => dm.Name == gift.Name))
                    {
                        db.DailyMissions.Add(gift);
                    }
                }
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role.Name).GetAwaiter().GetResult())
                    {
                        roleManager.CreateAsync(new Role() { Name = role.Name }).GetAwaiter().GetResult();
                    }
                }
                foreach (var account in superAdminAccounts)
                {
                    var newAccount = new Account()
                    {
                        UserName = account.Username,
                        PhoneNumber = account.PhoneNumber,
                        Email = account.Email,
                        Address = account.Address
                    };
                    if (!db.Accounts.Any(acc => acc.UserName == newAccount.UserName))
                    {
                        userManager.CreateAsync(newAccount, account.Password).GetAwaiter().GetResult();
                        userManager.AddToRoleAsync(newAccount, "learner").GetAwaiter().GetResult();
                    }
                }
                
                //create admin User
                if (!db.Accounts.Any(acc => acc.UserName == "Admin"))
                {
                    var admin = new Account()
                    {
                        UserName = "Admin"
                    };
                    var result = userManager.CreateAsync(admin, "Thanhpro1@").Result;
                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(admin, "superadmin").GetAwaiter().GetResult();
                    }
                }
                db.SaveChanges();
            }
        }
    }
}