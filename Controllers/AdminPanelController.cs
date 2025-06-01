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
        var users =await _userManager.Users.ToListAsync();
        var userList = await Task.WhenAll(users.Select(async user=>new UserViewModel
        {
            Id=user.Id,
            Email=user.Email,
            Roles=await _userManager.GetRolesAsync(user)
        }));
        return View(userList.ToList());
    }
}