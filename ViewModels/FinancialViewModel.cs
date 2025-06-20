namespace Inventorymanagement.ViewModels;

public class FinancialViewModel
{
    public decimal TotalMoneyAmount { get; set; }
    public Dictionary<string, int> soldItems { get; set; }
    public Dictionary<string,decimal> salesByUsers { get; set; }
}