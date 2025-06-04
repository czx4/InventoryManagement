using InventoryManagment.Data;
using InventoryManagment.Models;
using InventoryManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagment.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SuppliersController(ApplicationDbContext context,UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    // GET
    public async Task<IActionResult> Index()
    {
        var supplierList = await _context.Suppliers
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
        var supplier = await _context.Suppliers
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
    [ValidateAntiForgeryToken]
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

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

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
        var supplier =await _context.Suppliers.FindAsync(id);
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSupplierConfirmed(int id)
    {
        var supplier =await _context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        try
        {
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
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
        var supplier =await _context.Suppliers.FindAsync(id);
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

    [HttpPost,ActionName("Edit")]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditConfirm(SupplierViewModel svm)
    {
        if (!ModelState.IsValid) return View(svm);
        var supplier = await _context.Suppliers.FindAsync(svm.Id);
        if (supplier == null) return NotFound();
        try
        {
            supplier.Name = svm.Name;
            supplier.ContactName = svm.ContactName;
            supplier.Email = svm.Email;
            supplier.Phone = svm.Phone;
            supplier.Address = svm.Address;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("",ex.Message);
            return View(svm);
        }
    }
}