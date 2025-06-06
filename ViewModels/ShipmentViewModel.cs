using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagment.ViewModels;

public class ShipmentItemViewModel
{
    public int Id { get; set; }
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    [Required]
    [Display(Name = "Product")]
    public int ProductId { get; set; }
    
}
public class ShipmentViewModel
{
    public int? SupplierId { get; set; }
    public List<ShipmentItemViewModel> Shipments { get; set; } = new();

    public IEnumerable<SelectListItem>? Products { get; set; } = new List<SelectListItem>();
}
