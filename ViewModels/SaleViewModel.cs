using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagment.ViewModels;

public class SaleViewModel
{
    public int Id { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CreatedByUserName { get; set; }
    public IEnumerable<SaleLineItemViewModel>? SaleLineItems { get; set; } = new List<SaleLineItemViewModel>();
}

public class SaleLineItemViewModel
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    [MaxLength(300)]
    public string? ProductName { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    [Range(0.01,double.MaxValue, ErrorMessage = "Unit Price must be greater than zero.")]
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public IEnumerable<SelectListItem>? Products { get; set; }
}