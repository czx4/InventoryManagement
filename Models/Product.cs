using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventorymanagement.Models;

public class Product
{
    public int Id { get; init; }
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

    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [DataType(DataType.Date)]
    public DateTime? LastUpdated { get; set; } = DateTime.UtcNow;
    
    public int CategoryId { get; set; }
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }

    public int SupplierId { get; set; }
    [ForeignKey(nameof(SupplierId))]
    public Supplier Supplier { get; set; }
    public ICollection<Shipment> Shipments { get; set; }
}
