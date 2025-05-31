using System.Security.Claims;
using InventoryManagment.Models;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagment.Middleware;

public class PasswordChangeEnforcementMiddleware
{
    private readonly RequestDelegate _next;

    public PasswordChangeEnforcementMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        // Check if user is authenticated
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Get user id from claims
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                var user = await userManager.FindByIdAsync(userId);

                if (user != null && user.MustChangePassword)
                {
                    var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

                    if (!path.StartsWith("/identity/account/manage/changepassword") &&
                        !path.StartsWith("/identity/account/logout"))
                    {
                        context.Response.Redirect("/Identity/Account/Manage/ChangePassword");
                        return;
                    }
                }
            }
        }

        await _next(context);
    }
}