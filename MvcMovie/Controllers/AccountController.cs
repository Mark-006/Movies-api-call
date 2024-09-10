using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MvcMovie.Data;
using MvcMovie.Models;
using System.Security.Claims;

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

                // Add the new AccountUser entity to the database with the selected role
                _applicationcontext.AccountUsers.Add(model);
                await _applicationcontext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction(nameof(Login));  // Ensure you have a Login action
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
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.Role, user.Role) // Include the role in the claims
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
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Specify the scheme to clear authentication cookies

            // Redirect to the first page, typically the Index action of the Home controller.
            return RedirectToAction("Index", "Home");
        }
    }
}
