using System.ComponentModel.DataAnnotations;

namespace InventoryManagment.Models;

public class CreateUserViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [Required]
    public string Role { get; set; }

    public List<string> AvailableRoles { get; set; } = new();
}