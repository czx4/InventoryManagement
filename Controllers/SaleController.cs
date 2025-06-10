using System.Security.Claims;
using InventoryManagment.Data;
using InventoryManagment.Models;
using InventoryManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagment.Controllers;
[Authorize]
public class SaleController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string searchTerm)
    {
        var query = context.Sales
            .Include(s => s.CreatedByUser)
            .Include(s=>s.SaleLineItems)
                .ThenInclude(sl=>sl.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s =>
                s.Id.ToString().Contains(searchTerm) ||
                s.SaleDate.ToString().Contains(searchTerm) ||
                s.CreatedByUser.UserName.Contains(searchTerm) ||
                s.TotalAmount.ToString().Contains(searchTerm) ||
                s.SaleLineItems.Select(sl => sl.Product.Name).Contains(searchTerm));
        }
        
        var sales =await query
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new SaleListViewModel
            {
                Id = s.Id,
                SaleDate = s.SaleDate,
                TotalAmount = s.TotalAmount,
                CreatedByUserName = s.CreatedByUser.UserName,
                Products = s.SaleLineItems
                    .Select(sl=>sl.Product.Name)
                    .ToList()
            })
            .ToListAsync();
        return View(sales);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var sale =await context.Sales
            .Include(s => s.CreatedByUser)
            .Include(s => s.SaleLineItems)
            .FirstOrDefaultAsync(s=>s.Id==id);
        if (sale == null) return NotFound();
        
        var viewmod = new SaleViewModel
        {
            Id = sale.Id,
            SaleDate = sale.SaleDate,
            TotalAmount = sale.TotalAmount,
            CreatedByUserName = sale.CreatedByUser.UserName,
            SaleLineItems = sale.SaleLineItems.Select(s => new SaleLineItemViewModel
            {
                Id = s.Id,
                ProductName = context.Products
                    .Where(p => p.Id == s.ProductId)
                    .Select(p => p.Name)
                    .FirstOrDefault(),
                Quantity = s.Quantity,
                UnitPrice = s.UnitPrice,
                TotalPrice = s.TotalPrice
            }) 
        };
        return View(viewmod);
    }

    [HttpGet]
    public async Task<IActionResult> NewSale()
    {
        var model = new SaleViewModel();
        await PopulateSaleDropdown(model);
        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> NewSale(SaleViewModel svm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSaleDropdown(svm);
            return View(svm);
        }
        try
        {
            var noproducts = new List<int>();
            
            var productIds = svm.SaleLineItems.Select(sli => sli.ProductId).Distinct().ToList();
            var shipmentsByProduct = await context.Shipments
                .Where(s => productIds.Contains(s.ProductId))
                .OrderBy(s => s.ExpiryDate)
                .ToListAsync();

            // Fetch product prices 
            var productPrices = await context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Price);
            
            
            foreach (var product in svm.SaleLineItems )
            {

                var quant = product.Quantity;
                var prodId = product.ProductId;
                var shipments = shipmentsByProduct.Where(s => s.ProductId == prodId).ToList();
                
                if (shipments.Sum(s => s.Quantity) < quant)
                {
                    noproducts.Add(prodId);
                    continue;
                }
                foreach (var shipment in shipments) 
                {
                    if (quant <= 0) break;

                    if (shipment.Quantity >= quant)
                    {
                        shipment.Quantity -= quant;
                        quant = 0;
                    }
                    else
                    {
                        quant -= shipment.Quantity;
                        context.Shipments.Remove(shipment);
                    }
                }
                
            }

            if (noproducts.Any())
            {
                ViewBag.ProductsNotAdded= await context.Products
                    .Where(p => noproducts.Contains(p.Id))
                    .Select(p => p.Name)
                    .ToListAsync();
                await PopulateSaleDropdown(svm);
                return View(svm);
            }
            
            var saleLineItems = svm.SaleLineItems
                .Where(item => !noproducts.Contains(item.ProductId))
                .Select(item => new SaleLineItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = productPrices[item.ProductId]
                })
                .ToList();

            var totalAmount = saleLineItems.Sum(sli => sli.UnitPrice * sli.Quantity);

            var sale = new Sale
            {
                CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                SaleLineItems = saleLineItems,
                TotalAmount = totalAmount
            };
            
            context.Sales.Add(sale);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("",ex.Message);
            await PopulateSaleDropdown(svm);
            return View(svm);
        }
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var model =await context.Sales.FirstAsync(s=>s.Id==id);
        return View(model);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteSaleConfirmed(int id)
    {
        var sale =await context.Sales.Include(sale => sale.SaleLineItems).FirstAsync(s=>s.Id==id);
        if (sale == null) return NotFound();
        try
        {
            var product=sale.SaleLineItems.Select(sl => new { Id = sl.ProductId, Quant = sl.Quantity });
            foreach (var saleItem in product)
            {
                var shipment=await context.Shipments.FirstAsync(sh => sh.ProductId == saleItem.Id);
                shipment.Quantity += saleItem.Quant;  //todo in future add check if shipment for product exists and set the expiry date to correct one
            }
            context.Sales.Remove(sale);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(sale);
        }
    }
    private async Task PopulateSaleDropdown(SaleViewModel model)
    {
        
        var products = await context.Products.ToListAsync();
        ViewBag.ProductPrices=products.ToDictionary(p => p.Id, p => p.Price);
        if(model.SaleLineItems==null||!model.SaleLineItems.Any())
        {
            model.SaleLineItems = new List<SaleLineItemViewModel>
            {
                new()
                {
                    Products = products
                        .Select(p => new SelectListItem
                        {
                            Value = p.Id.ToString(),
                            Text = p.Name
                        }).ToList()
                }
            };
        }
        else
        {
            // Populate dropdowns for existing line items
            foreach (var item in model.SaleLineItems)
            {
                item.Products = products
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    }).ToList();
            }
        }
    }
}