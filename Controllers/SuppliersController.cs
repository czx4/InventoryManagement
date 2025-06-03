using InventoryManagment.Data;
using InventoryManagment.Models;
using InventoryManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        var model = new CreateSupplierViewModel();
        return View(model);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSupplierViewModel csvm)
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
}