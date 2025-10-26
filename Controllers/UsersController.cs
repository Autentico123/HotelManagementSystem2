using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem2.Data;
using HotelManagementSystem2.Models;
using HotelManagementSystem2.Models.ViewModels;

namespace HotelManagementSystem2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Roles = roles.ToList(),
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnd = user.LockoutEnd
                });
            }

            return View(userViewModels);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Roles = roles.ToList(),
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd
            };

            return View(viewModel);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                CurrentRoles = userRoles.ToList(),
                AllRoles = allRoles!
            };

            return View(viewModel);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update roles if SelectedRole is provided
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }

                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Reload roles if validation fails
            model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync()!;
            model.CurrentRoles = (await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(id)!)).ToList();
            return View(model);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting yourself
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["Error"] = "You cannot delete your own account!";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Roles = roles.ToList()
            };

            return View(viewModel);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting yourself
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["Error"] = "You cannot delete your own account!";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "User deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Error deleting user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Users/LockUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Lock user for 100 years (effectively permanent)
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            TempData["Success"] = $"User {user.FullName} has been locked successfully!";

            return RedirectToAction(nameof(Index));
        }

        // POST: Users/UnlockUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.SetLockoutEndDateAsync(user, null);
            TempData["Success"] = $"User {user.FullName} has been unlocked successfully!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Users/ChangePassword/5
        public async Task<IActionResult> ChangePassword(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ChangePasswordViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? ""
            };

            return View(model);
        }

        // POST: Users/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Remove password and add new one (admin reset)
                await _userManager.RemovePasswordAsync(user);
                var result = await _userManager.AddPasswordAsync(user, model.NewPassword);

                if (result.Succeeded)
                {
                    TempData["Success"] = "Password changed successfully!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
    }
}
