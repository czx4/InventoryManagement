using System.ComponentModel.DataAnnotations;

namespace InventoryManagment.ViewModels;

public class EditUserViewModel
{
    public string Id { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    public IList<string> Roles { get; set; }
    [Display(Name = "New Password")]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }
    public bool MustChangePassword { get; set; }
    public List<string> AvailableRoles { get; set; } = new();
}