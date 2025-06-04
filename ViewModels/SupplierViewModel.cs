using System.ComponentModel.DataAnnotations;

namespace InventoryManagment.ViewModels;

public class SupplierViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Supplier name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; }

    [Display(Name = "Contact Person")]
    [StringLength(100, ErrorMessage = "Contact name cannot exceed 100 characters.")]
    public string? ContactName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number.")]
    [StringLength(25, ErrorMessage = "Phone number cannot exceed 25 characters.")]
    public string? Phone { get; set; }

    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
    public string? Address { get; set; }
}
