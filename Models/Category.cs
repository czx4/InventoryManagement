using System.ComponentModel.DataAnnotations;

namespace Inventorymanagement.Models;

public class Category
{
    public int Id { get; init; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; }
}