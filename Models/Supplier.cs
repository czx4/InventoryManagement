using System.ComponentModel.DataAnnotations;

namespace InventoryManagment.Models;

public class Supplier
{
    public int Id { get; init; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    [MaxLength(100)]
    public string? ContactName { get; set; }
    [MaxLength(254)]
    public string? Email { get; set; }
    [MaxLength(20)]
    public string? Phone { get; set; }
    [MaxLength(200)]
    public string? Address { get; set; }

    public ICollection<Product> Products { get; set; }
}