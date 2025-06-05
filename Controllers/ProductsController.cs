using InventoryManagment.Data;
using InventoryManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagment.Controllers;
[Authorize]
public class ProductsController(ApplicationDbContext context) : Controller
{
    // GET
    public async Task<IActionResult> Index(string searchTerm)
    {
        var query = context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Shipments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p =>
                p.Name.Contains(searchTerm) ||
                p.SKU.Contains(searchTerm) ||
                p.Category.Name.Contains(searchTerm) ||
                p.Supplier.Name.Contains(searchTerm));
        }

        var products = await query
            .Select(p => new ProductListViewModel
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Price = p.Price,
                CategoryName = p.Category.Name,
                SupplierName = p.Supplier.Name,
                QuantityInStock = p.Shipments.Sum(s => s.Quantity),
                IsLowStock=p.Shipments.Sum(s=>s.Quantity)<=p.ReorderLevel
            })
            .ToListAsync();

        return View(products);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var product = await context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Shipments)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (product == null) return NotFound();
        var viewMod = new ProductDetailsViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            QuantityInStock = product.Shipments.Sum(s=>s.Quantity),
            IsLowStock=product.Shipments.Sum(s=>s.Quantity)<=product.ReorderLevel,
            ReorderLevel = product.ReorderLevel,
            CreatedAt = product.CreatedAt,
            LastUpdated = product.LastUpdated,
            CategoryName = product.Category.Name,
            SupplierName = product.Supplier.Name,
            Shipments = product.Shipments.Select(s=>new ShipmentViewModel
            {
                Id = s.Id,
                Quantity = s.Quantity,
                ExpiryDate = s.ExpiryDate,
                ReceivedDate = s.ReceivedDate
            }).ToList()
        };
        return View(viewMod);
    }
}