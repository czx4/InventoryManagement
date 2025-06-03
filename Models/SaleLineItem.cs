using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagment.Models;

public class SaleLineItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }
    [ForeignKey(nameof(SaleId))]
    public Sale Sale { get; set; }
    
    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }  // Price at time of sale

    public decimal TotalPrice => Quantity * UnitPrice;
}
