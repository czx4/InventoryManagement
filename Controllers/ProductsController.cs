using InventoryManagment.Data;
using InventoryManagment.Models;
using InventoryManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            Shipments = product.Shipments.Select(s=>new ShipmentListViewModel
            {
                Id = s.Id,
                Quantity = s.Quantity,
                ExpiryDate = s.ExpiryDate,
                ReceivedDate = s.ReceivedDate
            }).ToList()
        };
        return View(viewMod);
    }
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create()
    {
        var model =new ProductViewModel();
        await PopulateDropdowns(model);
        return View(model);
    }
    
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel pvm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(pvm);
            return View(pvm);
        }
        try
        {
            var product = new Product
            {
                Name = pvm.Name,
                Description = pvm.Description,
                SKU = pvm.SKU,
                Price = pvm.Price,
                ReorderLevel = pvm.ReorderLevel,
                CategoryId = pvm.CategoryId,
                SupplierId = pvm.SupplierId
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("",ex.Message);
            await PopulateDropdowns(pvm);
            return View(pvm);
        }
    }
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await context.Products
            .Include(p=>p.Category)
            .Include(p=>p.Supplier)
            .FirstOrDefaultAsync(p=>p.Id==id);
        if (product == null) return NotFound();
        var model = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            ReorderLevel = product.ReorderLevel,
            CategoryName = product.Category.Name,
            SupplierName = product.Supplier.Name
        };
        return View(model);
    }
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProductConfirmed(int id)
    {
        var product =await context.Products.FirstOrDefaultAsync(p=>p.Id==id);
        if (product == null) return NotFound();
        try
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("", ex.Message);
            var fullProduct =await context.Products
                .Include(p=>p.Category)
                .Include(p=>p.Supplier)
                .FirstOrDefaultAsync(p=>p.Id==id);
            if (fullProduct == null) return NotFound();
            return View(new ProductViewModel
            {
                Id = fullProduct.Id,
                Name = fullProduct.Name,
                Description = fullProduct.Description,
                SKU = fullProduct.SKU,
                Price = fullProduct.Price,
                ReorderLevel = fullProduct.ReorderLevel,
                CategoryName = fullProduct.Category.Name,
                SupplierName = fullProduct.Supplier.Name
            });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var product =await context.Products
            .Include(p=>p.Category)
            .Include(p=>p.Supplier)
            .FirstOrDefaultAsync(p=>p.Id==id);
        if (product == null) return NotFound();
        var model = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            ReorderLevel = product.ReorderLevel,
            SupplierId = product.SupplierId,
            CategoryId = product.CategoryId
        };
        await PopulateDropdowns(model);
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductViewModel pvm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(pvm);
            return View(pvm);
        }
        var product =await context.Products
            .FirstOrDefaultAsync(p=>p.Id==pvm.Id);
        if (product == null) return NotFound();
        try
        {
            product.Name = pvm.Name;
            product.Description = pvm.Description;
            product.SKU = pvm.SKU;
            product.Price = pvm.Price;
            product.ReorderLevel = pvm.ReorderLevel;
            product.CategoryId = pvm.CategoryId;
            product.SupplierId = pvm.SupplierId;
            product.LastUpdated=DateTime.Now;
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("",ex.Message);
            await PopulateDropdowns(pvm);
            return View(pvm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> AddShipment(int id) //supplier id
    {
        var model = new ShipmentViewModel { SupplierId = id };
        await PopulateShipmentDropdown(model);
        return View(model);
    }
    

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddShipment(ShipmentViewModel svm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateShipmentDropdown(svm);
            return View(svm);
        }
        try
        {
            foreach (var item in svm.Shipments)
            {
                var shipment = new Shipment
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    ExpiryDate = item.ExpiryDate
                };
                context.Shipments.Add(shipment);
            }
            
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("",ex.Message);
            await PopulateShipmentDropdown(svm);
            return View(svm);
        }
    }
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> EditShipment(int id)
    {
        var shipment =await context.Shipments.FindAsync(id);
        if (shipment == null) return NotFound();
        var model = new ShipmentItemViewModel
        {
            Id = shipment.Id,
            Quantity = shipment.Quantity,
            ExpiryDate = shipment.ExpiryDate
        };
        return View(model);
    }
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> EditShipment(ShipmentItemViewModel sivm)
    {
        if (!ModelState.IsValid) return View(sivm);
        var shipment =await context.Shipments.FindAsync(sivm.Id);
        if (shipment == null) return NotFound();
        try
        {
            shipment.Quantity = sivm.Quantity;
            shipment.ExpiryDate = sivm.ExpiryDate;
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("",ex.Message);
            return View(sivm);
        }
    }
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteShipment(int id)
    {
        var shipment =await context.Shipments.FindAsync(id);
        if (shipment == null) return NotFound();
        var model = new ShipmentItemViewModel
        {
            Id = shipment.Id,
            Quantity = shipment.Quantity,
            ExpiryDate = shipment.ExpiryDate
        };
        return View(model);
    }
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost, ActionName("DeleteShipment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteShipmentConfirmed(int id)
    {
        var shipment =await context.Shipments.FindAsync(id);
        if (shipment == null) return NotFound();
        try
        {
            context.Shipments.Remove(shipment);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new ShipmentItemViewModel
            {
                Id = shipment.Id,
                Quantity = shipment.Quantity,
                ExpiryDate = shipment.ExpiryDate
            });
        }
    }
    private async Task PopulateDropdowns(ProductViewModel model)
    {
        model.Categories = await context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();
        model.Suppliers = await context.Suppliers
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            .ToListAsync();
    }

    private async Task PopulateShipmentDropdown(ShipmentViewModel model)
    {
        model.Products = await context.Products
            .Where(p => p.SupplierId == model.SupplierId)
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
            .ToListAsync();
    }
}