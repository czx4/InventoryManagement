using System.ComponentModel.DataAnnotations;

namespace InventoryManagment.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public string? ContactName { get; set; }
    
    public string? Email { get; set; }
    
    public string? Phone { get; set; }
    
    public string? Address { get; set; }

    public ICollection<Product> Products { get; set; }
}