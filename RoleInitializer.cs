using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using CollectionsApp.Models;


namespace CollectionsApp
{
    public class RoleInitializer
    {
        public static async Task InitializeAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "admin", "active user"};
            foreach(string roleName in roles)
            {
                await AddRoleIfNotExists(roleManager, roleName);
            }
            await AddAdminUserIfNotExists(userManager, "admin@mail.ru", "admin", "admin"); 
        }

        public static async Task AddRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (await roleManager.FindByNameAsync(roleName) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        public static async Task AddAdminUserIfNotExists(UserManager<IdentityUser> userManager, string adminEmail, string adminPassword, string adminRoleName)
        {
            if (await userManager.FindByNameAsync(adminEmail) == null)
            {
                IdentityUser adminUser = new IdentityUser { Email = adminEmail, UserName = adminEmail, EmailConfirmed = true };
                IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(adminUser, new string[]{ adminRoleName, "active user" });
                }
            }
        }
    }
}
