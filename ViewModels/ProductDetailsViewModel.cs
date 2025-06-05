using System.ComponentModel.DataAnnotations;
using InventoryManagment.Models;

namespace InventoryManagment.ViewModels;

public class ProductDetailsViewModel
{
    public int Id { get; set; }
    
    public string Name { get; set; }

    public string? Description { get; set; }

    public string? SKU { get; set; } 
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
    [Display(Name = "Quantity in stock")]
    public int QuantityInStock { get; set; }
    [Display(Name = "Reorder stock level")]
    public bool IsLowStock { get; set; }
    public int ReorderLevel { get; set; } 
    [Display(Name = "Created at")]
    public DateTime CreatedAt { get; set; }
    [Display(Name = "Last updated at")]
    public DateTime? LastUpdated { get; set; }
    [Display(Name = "Category")]
    public string CategoryName { get; set; }
    [Display(Name = "Supplier")]
    public string SupplierName { get; set; }
    
    public IEnumerable<ShipmentViewModel> Shipments { get; set; } = new List<ShipmentViewModel>();
}
public class ShipmentViewModel
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    [Display(Name = "Expires at")]
    public DateTime? ExpiryDate { get; set; }
    [Display(Name = "Received at")]
    public DateTime ReceivedDate { get; set; }
}