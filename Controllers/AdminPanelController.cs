using InventoryManagment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagment.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class AdminPanelController:Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public AdminPanelController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }
    
    public async Task<IActionResult> Index()
    {
        var currentUser =await _userManager.GetUserAsync(User);
        var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
        ViewBag.IsManagerOnly = currentUserRoles.Contains("Manager") && !currentUserRoles.Contains("Admin");
        var users =await _userManager.Users.ToListAsync();
        var userList = await Task.WhenAll(users.Select(async user=>new UserViewModel
        {
            Id=user.Id,
            Email=user.Email,
            Roles=await _userManager.GetRolesAsync(user)
        }));
        return View(userList.ToList());
    }
    [HttpGet]
    public async  Task<IActionResult> CreateUser()
    {
        var availableRoles = await GetAvailableRolesForCurrentUser();
        var model = new CreateUserViewModel
        {
            AvailableRoles =availableRoles
        };
        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel createUserViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(createUserViewModel);
        }

        var user = new ApplicationUser { UserName = createUserViewModel.Email, Email = createUserViewModel.Email, EmailConfirmed = true };
        var result =await _userManager.CreateAsync(user, createUserViewModel.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, createUserViewModel.Role);
            return RedirectToAction("Index");
        }

        foreach (var error in result.Errors)
        {
            // Try to link the error to a specific field, for example Password validation errors:
            if (error.Code.Contains("Password"))
                ModelState.AddModelError("Password", error.Description);
            else if (error.Code.Contains("Email") || error.Code.Contains("UserName"))
                ModelState.AddModelError("Email", error.Description);
            else
                ModelState.AddModelError(string.Empty, error.Description); // fallback
        }
        createUserViewModel.AvailableRoles = await GetAvailableRolesForCurrentUser();
        return View(createUserViewModel);
    }
    
    private async Task<List<string>> GetAvailableRolesForCurrentUser()
    {
        var currentUser =await _userManager.GetUserAsync(User);
        var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
        if (currentUserRoles.Contains("Admin"))
        {
            return new List<string> { "Admin", "Manager", "Clerk" };
        }
        if (currentUserRoles.Contains("Manager"))
        {
            return new List<string> { "Clerk" };
        }
        return new List<string>();
    }
}