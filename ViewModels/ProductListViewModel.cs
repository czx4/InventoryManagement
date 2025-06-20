
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventorymanagement.ViewModels;

public class ProductListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? SKU { get; set; } // Stock Keeping Unit (unique code)
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
    [Display(Name = "Category")]
    public string CategoryName { get; set; }
    [Display(Name = "Supplier")]
    public string SupplierName { get; set; }
    [Display(Name = "Quantity in stock")]
    public int QuantityInStock { get; set; }
    public bool IsLowStock { get; set; }
    
    public IEnumerable<SelectListItem>? Suppliers { get; set; } = new List<SelectListItem>();
}