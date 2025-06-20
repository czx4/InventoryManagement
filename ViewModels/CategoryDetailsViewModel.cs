namespace Inventorymanagement.ViewModels;

public class CategoryDetailsViewModel
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    public IEnumerable<ProductSummaryViewModel> Products { get; set; } = new List<ProductSummaryViewModel>();
}
