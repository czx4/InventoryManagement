namespace InventoryManagment.ViewModels;


public class SupplierDetailsViewModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string? ContactName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }
    
    public IEnumerable<ProductSummaryViewModel> Products { get; set; } = new List<ProductSummaryViewModel>();
}

// A minimal view‐model to represent each product in the Details view.
