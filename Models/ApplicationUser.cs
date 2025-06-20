using Microsoft.AspNetCore.Identity;

namespace Inventorymanagement.Models;

public class ApplicationUser:IdentityUser
{
    public bool MustChangePassword { get; set; } = true;
}