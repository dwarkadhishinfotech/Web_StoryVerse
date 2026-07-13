using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities;
using StoryVerse.Core.Entities.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StoryVerse.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Administrator", "Author", "Premium" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public static async Task SeedDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        var authorEmail = "author@storyverse.com";
        var user = await userManager.FindByEmailAsync(authorEmail);
        
        if (user == null)
        {
            user = new ApplicationUser 
            { 
                UserName = authorEmail, 
                Email = authorEmail,
                FirstName = "Author",
                LastName = "User",
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(user, "Password123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Author");
            }
        }
    }
}
