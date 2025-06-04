using InventoryManagment.Data;
using InventoryManagment.Models;
using InventoryManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagment.Controllers;
[Authorize]
public class CategoriesController(ApplicationDbContext context) : Controller
{
    // GET
    public async Task<IActionResult> Index()
    {
        var categoriesList =await context.Categories
            .OrderBy(s => s.Name)
            .Select(s => new CategoryListViewModel
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync();
        return View(categoriesList);
    }
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var category = await context.Categories
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (category == null) return NotFound();
        var viewMod = new CategoryDetailsViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Products = category.Products.Select(p => new ProductSummaryViewModel
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
        var model = new CategoryViewModel();
        return View(model);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryViewModel cvm)
    {
        if (!ModelState.IsValid) return View(cvm);
        try
        {
            var category = new Category
            {
                Name = cvm.Name,
                Description = cvm.Description
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(cvm);
        }
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var category =await context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        var model = new CategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
        return View(model);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategoryConfirmed(int id)
    {
        var category =await context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        try
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var category =await context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        var model = new CategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryViewModel cvm)
    {
        if (!ModelState.IsValid) return View(cvm);
        var category = await context.Categories.FindAsync(cvm.Id);
        if (category == null) return NotFound();
        try
        {
            category.Name = cvm.Name;
            category.Description = cvm.Description;
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("",ex.Message);
            return View(cvm);
        }
    }
}