using System.ComponentModel.DataAnnotations;

namespace InventoryManagment.ViewModels;

public class CategoryViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; }

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }
}