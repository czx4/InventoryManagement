using InventoryManagment.Data;
using InventoryManagment.Models;
using InventoryManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagment.Controllers;

[Authorize]
public class SuppliersController(ApplicationDbContext context) : Controller
{
    // GET
    public async Task<IActionResult> Index()
    {
        var supplierList = await context.Suppliers
            .OrderBy(s => s.Name)
            .Select(s => new SupplierListItemViewModel
            {
                Id = s.Id,
                Name = s.Name,
                ContactName = s.ContactName,
                Email = s.Email,
                Phone = s.Phone
            })
            .ToListAsync();
        return View(supplierList);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var supplier = await context.Suppliers
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (supplier == null) return NotFound();
        var viewMod = new SupplierDetailsViewModel
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactName = supplier.ContactName,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            Products = supplier.Products.Select(p => new ProductSummaryViewModel
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU
            }).ToList()
        };
        return View(viewMod);
    }
    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public  IActionResult Create()
    {
        var model = new SupplierViewModel();
        return View(model);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    public async Task<IActionResult> Create(SupplierViewModel csvm)
    {
        if (!ModelState.IsValid) return View(csvm);
        try
        {
            var supplier = new Supplier
            {
                Name = csvm.Name,
                ContactName = csvm.ContactName,
                Email = csvm.Email,
                Phone = csvm.Phone,
                Address = csvm.Address
            };

            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(csvm);
        }
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var supplier =await context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        var model = new SupplierViewModel
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactName = supplier.ContactName,
            Address = supplier.Address,
            Email = supplier.Email,
            Phone = supplier.Phone
        };
        return View(model);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteSupplierConfirmed(int id)
    {
        var supplier =await context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        try
        {
            context.Suppliers.Remove(supplier);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new SupplierViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactName = supplier.ContactName,
                Address = supplier.Address,
                Email = supplier.Email,
                Phone = supplier.Phone
            });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var supplier =await context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        var model = new SupplierViewModel
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactName = supplier.ContactName,
            Address = supplier.Address,
            Email = supplier.Email,
            Phone = supplier.Phone
        };
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(SupplierViewModel svm)
    {
        if (!ModelState.IsValid) return View(svm);
        var supplier = await context.Suppliers.FindAsync(svm.Id);
        if (supplier == null) return NotFound();
        try
        {
            supplier.Name = svm.Name;
            supplier.ContactName = svm.ContactName;
            supplier.Email = svm.Email;
            supplier.Phone = svm.Phone;
            supplier.Address = svm.Address;
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("",ex.Message);
            return View(svm);
        }
    }
}