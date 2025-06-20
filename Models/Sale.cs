using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagment.Models;

public class Sale
{
    public int Id { get; init; }

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public string? CreatedByUserId { get; set; }  // To track who made the sale
    [ForeignKey(nameof(CreatedByUserId))]
    public ApplicationUser? CreatedByUser { get; set; }

    public ICollection<SaleLineItem> SaleLineItems { get; set; }
}