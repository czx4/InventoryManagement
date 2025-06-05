using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagment.ViewModels;

public class ProductViewModel
{
    public int Id { get; set; }
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    [StringLength(255)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? SKU { get; set; } // Stock Keeping Unit (unique code)

    [Required]
    [Range(0, double.MaxValue)]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; set; } // Alert threshold
    [Required]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }
    [Required]
    [Display(Name = "Supplier")]
    public int SupplierId { get; set; }
    public string? CategoryName { get; set; }
    public string? SupplierName { get; set; }
    public IEnumerable<SelectListItem>? Categories { get; set; }
    public IEnumerable<SelectListItem>? Suppliers { get; set; }


}