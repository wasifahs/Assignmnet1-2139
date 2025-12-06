using Assignment1_2139.Models;
using Microsoft.AspNetCore.Identity;

public static class DbInitializer
{
    public static async Task SeedRolesAndUsers(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 1️⃣ Create roles
        string[] roles = new[] { "Admin", "Organizer", "Attendee" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2️⃣ Create Admin user
        await CreateUserIfNotExists(userManager, "admin@etickets.com", "Admin User", "Admin@123", "0000000000", new DateTime(1990, 1, 1), "/images/user.png", "Admin");

        // 3️⃣ Create Organizer user
        await CreateUserIfNotExists(userManager, "organizer@etickets.com", "Organizer User", "Organizer@123", "1111111111", new DateTime(1992, 2, 2), "/images/user.png", "Organizer");

        // 4️⃣ Create Attendee user
        await CreateUserIfNotExists(userManager, "attendee@etickets.com", "Attendee User", "Attendee@123", "2222222222", new DateTime(1995, 3, 3), "/images/user.png", "Attendee");
    }

    private static async Task CreateUserIfNotExists(UserManager<ApplicationUser> userManager, string email, string fullName, string password, string phone, DateTime dob, string profilePictureUrl, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                PhoneNumber = phone,
                DateOfBirth = DateTime.SpecifyKind(dob, DateTimeKind.Utc),
                ProfilePictureUrl = profilePictureUrl,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, role);
        }
    }
}



