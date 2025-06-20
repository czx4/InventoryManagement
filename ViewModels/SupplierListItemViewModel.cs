using System.ComponentModel;

namespace Inventorymanagement.ViewModels;

public class SupplierListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    [DisplayName("Contact Name")]
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}