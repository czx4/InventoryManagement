using InventoryManagment.Data;
using InventoryManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagment.Controllers;
[Authorize(Roles = "Admin,Manager")]
public class ReportsController(ApplicationDbContext context) : Controller
{
    // GET
    public async Task<IActionResult> Index(DateTime? searchStartDate,DateTime? searchEndDate)
    {
        var query = context.Sales.AsNoTracking();

        if (searchStartDate.HasValue) query = query.Where(s => s.SaleDate >= searchStartDate);
        
        if (searchEndDate.HasValue) query = query.Where(s => s.SaleDate <= searchEndDate);
        
        var totalMoneyAmount =await query.SumAsync(s => s.TotalAmount);
        
        var soldItems =await query
            .SelectMany(s => s.SaleLineItems)
            .GroupBy(sl => sl.Product.Name)
            .Select(sl => new { ProductName = sl.Key, Quantity = sl.Sum(sli => sli.Quantity) })
            .OrderByDescending(sli=>sli.Quantity)
            .ToDictionaryAsync(sli => sli.ProductName, sli => sli.Quantity);
        
        var salesMadeByUsers =await query
            .GroupBy(s => s.CreatedByUser.UserName)
            .ToDictionaryAsync(s =>s.Key,s => s.Sum(t => t.TotalAmount));
        var model = new FinancialViewModel
        {
            TotalMoneyAmount = totalMoneyAmount,
            soldItems = soldItems,
            salesByUsers = salesMadeByUsers
        };
        return View(model);
    }
}