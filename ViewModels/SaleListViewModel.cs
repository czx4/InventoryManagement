
namespace Inventorymanagement.ViewModels;

public class SaleListViewModel
{
    public int Id { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CreatedByUserName { get; set; }
    
    public IEnumerable<string>? Products { get; set; }
}