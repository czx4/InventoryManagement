using Microsoft.AspNetCore.Identity;

namespace InventoryManagment.Models;

public class ApplicationUser:IdentityUser
{
    public bool MustChangePassword { get; set; } = true;
}