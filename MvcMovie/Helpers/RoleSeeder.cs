//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.DependencyInjection;
//using System.Threading.Tasks;

//namespace MvcMovie.Helpers // Ensure this matches your namespace
//{
//    public static class RoleSeeder
//    {
//        public static async Task AssignRoles(IServiceProvider services)
//        {
//            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

//            // Example user creation and role assignment
//            var user = await userManager.FindByEmailAsync("admin@example.com");
//            if (user == null)
//            {
//                user = new IdentityUser { UserName = "admin@example.com", Email = "admin@example.com" };
//                await userManager.CreateAsync(user, "Password@123");

//                // Assign role to the user
//                await userManager.AddToRoleAsync(user, "Admin");
//            }
//        }

//        internal static async Task SeedRolesAsync(IServiceProvider services)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}