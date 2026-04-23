using IT15_LabExam_Bais.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IT15_LabExam_Bais.Controllers;

public class AccountController(
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Users()
    {
        var users = await userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var items = new List<UserListItemViewModel>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            items.Add(new UserListItemViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Unassigned"
            });
        }

        return View(items);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditUserRole(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);
        return View(new EditUserRoleViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? "Staff"
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUserRole(EditUserRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByIdAsync(model.UserId);
        if (user is null)
        {
            return NotFound();
        }

        if (!await roleManager.RoleExistsAsync(model.Role))
        {
            ModelState.AddModelError(nameof(EditUserRoleViewModel.Role), "Selected role does not exist.");
            return View(model);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await userManager.AddToRoleAsync(user, model.Role);
        TempData["SuccessMessage"] = "User role updated successfully.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(new ResetUserPasswordViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetUserPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByIdAsync(model.UserId);
        if (user is null)
        {
            return NotFound();
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        TempData["SuccessMessage"] = "Password reset successfully.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var signInResult = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (signInResult.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await roleManager.RoleExistsAsync(model.Role))
        {
            ModelState.AddModelError(nameof(RegisterViewModel.Role), "Selected role does not exist.");
            return View(model);
        }

        var existingUser = await userManager.FindByEmailAsync(model.Email);
        if (existingUser is not null)
        {
            ModelState.AddModelError(nameof(RegisterViewModel.Email), "Email already exists.");
            return View(model);
        }

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await userManager.AddToRoleAsync(user, model.Role);
        TempData["SuccessMessage"] = "Account created successfully.";
        return RedirectToAction("Index", "Students");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
