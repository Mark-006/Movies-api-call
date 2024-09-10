using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMovie.Controllers
{
    [Authorize(Roles = "Admin")]  // Only Admins can access this controller
    public class RolesController : Controller
    {
        private readonly RolesDbContext _rolescontext;

        public RolesController(RolesDbContext rolescontext)
        {
            _rolescontext = rolescontext;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch roles from the database
                var roles = await _rolescontext.Roles.ToListAsync();

                // Pass success message if available
                ViewBag.SuccessMessage = TempData["SuccessMessage"]?.ToString();

                // Ensure Model is not null
                if (roles == null)
                {
                    roles = new List<Role>(); // Or an appropriate empty list based on your application
                }

                return View(roles);
            }
            catch (UnauthorizedAccessException)
            {
                // Redirect to an access denied view if unauthorized access occurs
                return View("AccessDenied");
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logging framework here)
                Console.WriteLine($"An error occurred: {ex.Message}");

                // Optionally, you could redirect to a general error view
                return View("Error");
            }
        }



        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }


        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoleName")] Role role)
        {
            if (ModelState.IsValid)
            {
                _rolescontext.Roles.Add(role);
                await _rolescontext.SaveChangesAsync();

                // Set success message in TempData
                TempData["SuccessMessage"] = "Role created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }




        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _rolescontext.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }


        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoleName")] Role role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _rolescontext.Roles.Update(role);
                    await _rolescontext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoleExists(role.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }


        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _rolescontext.Roles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _rolescontext.Roles.FindAsync(id);
            _rolescontext.Roles.Remove(role);
            await _rolescontext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool RoleExists(int id)
        {
            return _rolescontext.Roles.Any(e => e.Id == id);
        }
    }
}
