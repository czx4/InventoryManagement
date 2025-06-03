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
    [HttpGet]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        var model = new UserViewModel {Id = user.Id,Email = user.Email,Roles =roles};
        return View(model);
    }
    [HttpPost,ActionName("DeleteUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(string id)
    {
        var deleteUser = await _userManager.FindByIdAsync(id);
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || deleteUser == null) return NotFound();
        
        var currentUserRoles =await _userManager.GetRolesAsync(currentUser);
        var deleteUserRoles =await _userManager.GetRolesAsync(deleteUser);

        if (currentUserRoles.Contains("Admin"))
        {
            await _userManager.DeleteAsync(deleteUser);
        }
        else if (currentUserRoles.Contains("Manager") &&
                 !deleteUserRoles.Contains("Admin") && 
                 !deleteUserRoles.Contains("Manager"))
        {
            await _userManager.DeleteAsync(deleteUser);
        }
        else
        {
            TempData["Error"] = "You do not have permission to delete this user.";
        }
        return RedirectToAction("Index");
    }
    [HttpGet]
    public async Task<IActionResult> EditUser(string? id)
    {
        if (id == null) return NotFound();
        var user = await _userManager.FindByIdAsync(id);
        var currentUser =await _userManager.GetUserAsync(User);
        if (user == null||currentUser==null)
            return NotFound();
        if (await IsUserBlockedFromEditing(currentUser, user))
            return Forbid();
        var roles = await _userManager.GetRolesAsync(user);
        var availableRoles = await GetAvailableRolesForCurrentUser();
        var model = new EditUserViewModel {Id = user.Id,Email = user.Email,Roles =roles,AvailableRoles = availableRoles};
        return View(model);
    }

    [HttpPost,ActionName("EditUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUserConfirm(EditUserViewModel editUserViewModel)
    {
        var updatedUser = await _userManager.FindByIdAsync(editUserViewModel.Id);
        var currentUser = await _userManager.GetUserAsync(User);
        if (updatedUser == null||currentUser==null) return NotFound();
        if (await IsUserBlockedFromEditing(currentUser, updatedUser))
            return Forbid();
        var currentRoles = await _userManager.GetRolesAsync(updatedUser);
        var allowedRoles = await GetAvailableRolesForCurrentUser();
        var selectedRoles = editUserViewModel.Roles;
        
//  Reject any roles that are not in the allowed list
        if (selectedRoles.Except(allowedRoles).Any())
        {
            ModelState.AddModelError("Roles", "You tried to assign roles you're not authorized to set.");
            editUserViewModel.AvailableRoles = allowedRoles;
            return View(editUserViewModel);
        }
        
        //handle role change
        var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
        var rolesToRemove = currentRoles.Except(selectedRoles).ToList();

        if (rolesToAdd.Any())
            await _userManager.AddToRolesAsync(updatedUser, rolesToAdd);
        if (rolesToRemove.Any())
            await _userManager.RemoveFromRolesAsync(updatedUser, rolesToRemove);
        
        bool mustBeUpdated = false; //determines if user has to be updated
        //handle password change
        if (!string.IsNullOrEmpty(editUserViewModel.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(updatedUser);
            var result = await _userManager.ResetPasswordAsync(updatedUser, token, editUserViewModel.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("NewPassword", error.Description);
                editUserViewModel.AvailableRoles = allowedRoles;
                return View(editUserViewModel);
            }
            // ✅ Automatically set MustChangePassword = true
            updatedUser.MustChangePassword = true;
            // You must update the user in the DB
            mustBeUpdated = true;
        }
        //handle email change
        if (!string.Equals(editUserViewModel.Email,updatedUser.Email,StringComparison.OrdinalIgnoreCase))
        {
            var setEmailResult = await _userManager.SetEmailAsync(updatedUser, editUserViewModel.Email);
            if(!setEmailResult.Succeeded)
            {
                foreach (var error in setEmailResult.Errors)
                    ModelState.AddModelError("Email",error.Description);
                editUserViewModel.AvailableRoles = allowedRoles;
                return View(editUserViewModel);
            }
            //handle username change
            var setUsernameResult = await _userManager.SetUserNameAsync(updatedUser, editUserViewModel.Email);
            if(!setUsernameResult.Succeeded)
            {
                foreach (var error in setUsernameResult.Errors)
                    ModelState.AddModelError("Email", error.Description);
                editUserViewModel.AvailableRoles = allowedRoles;
                return View(editUserViewModel);
            }
            mustBeUpdated = true;
        }
        //handle the flag that the user must change password
        if (editUserViewModel.MustChangePassword)
        {
            updatedUser.MustChangePassword = true;
            mustBeUpdated = true;
        }
        //checks if user needs to be updated
        if(mustBeUpdated)
        {
            var updateResult=await _userManager.UpdateAsync(updatedUser);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(String.Empty, error.Description);
                editUserViewModel.AvailableRoles = allowedRoles;
                return View(editUserViewModel);
            }
        }
        return RedirectToAction("Index");
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
    private async Task<bool> IsUserBlockedFromEditing(ApplicationUser currentUser, ApplicationUser targetUser)
    {
        var currentRoles = await _userManager.GetRolesAsync(currentUser);
        var targetRoles = await _userManager.GetRolesAsync(targetUser);

        bool currentIsNotAdmin = !currentRoles.Contains("Admin");
        bool targetIsPrivileged = targetRoles.Any(r => r == "Admin" || r == "Manager");

        return currentIsNotAdmin && targetIsPrivileged;
    }
}