using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MvcMovie.Data;
using MvcMovie.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MvcMovie.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _applicationcontext;

        public AccountController(ApplicationDbContext applicationcontext)
        {
            _applicationcontext = applicationcontext;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AccountUser model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (_applicationcontext.AccountUsers.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Email already in use.");
                    return View(model);
                }

                // Directly store the password
                // No need to set PasswordHash or modify Password field

                // Add the new AccountUser entity to the database
                _applicationcontext.AccountUsers.Add(model);
                await _applicationcontext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction(nameof(Login));
            }
            return View(model);
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _applicationcontext.AccountUsers
                    .FirstOrDefault(u => u.Email == model.Input.Email);

                if (user != null && user.Password == model.Input.Password)
                {
                    // Set up authentication cookie here
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email)
                // Add other claims as needed
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid login attempt.");
            }
            return View(model);
        }








        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Handle logout (e.g., clearing cookies or session)
            HttpContext.SignOutAsync(); // Optional: Clears any authentication cookies or sessions.

            // Redirect to the first page, typically the Index action of the Home controller.
            return RedirectToAction("Index", "Home");
        }

    }
}
