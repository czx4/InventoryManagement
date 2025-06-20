using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagment.Models;

public class Shipment
{
    public int Id { get; init; }
    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
    [DataType(DataType.Date)]
    public DateTime? ExpiryDate { get; set; }
    [DataType(DataType.Date)]
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
}