using System.Security.Claims;
using InventoryManagment.Data;
using InventoryManagment.Models;
using InventoryManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagment.Controllers;

[Authorize]
public class SaleController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string searchTerm)
    {
        var query = context.Sales
            .Include(s => s.CreatedByUser)
            .Include(s => s.SaleLineItems)
            .ThenInclude(sl => sl.Product)
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

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new SaleListViewModel
            {
                Id = s.Id,
                SaleDate = s.SaleDate,
                TotalAmount = s.TotalAmount,
                CreatedByUserName = s.CreatedByUser.UserName,
                Products = s.SaleLineItems
                    .Select(sl => sl.Product.Name)
                    .ToList()
            })
            .ToListAsync();
        return View(sales);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var sale = await context.Sales
            .Include(s => s.CreatedByUser)
            .Include(s => s.SaleLineItems)
            .FirstOrDefaultAsync(s => s.Id == id);
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
        if (!ModelState.IsValid || svm.SaleLineItems.IsNullOrEmpty())
        {
            await PopulateSaleDropdown(svm);
            return View(svm);
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
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

            var expiry = new Dictionary<int, DateTime?>();
            foreach (var product in svm.SaleLineItems)
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
                        if (!expiry.ContainsKey(shipment.ProductId))
                            expiry.Add(shipment.ProductId, shipment.ExpiryDate);
                    }
                    else
                    {
                        quant -= shipment.Quantity;
                        if (!expiry.ContainsKey(shipment.ProductId))
                            expiry.Add(shipment.ProductId, shipment.ExpiryDate);
                        if (shipment.Quantity == 0) context.Shipments.Remove(shipment);
                    }
                }

            }

            if (noproducts.Any())
            {
                ViewBag.ProductsNotAdded = await context.Products
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
                    UnitPrice = productPrices[item.ProductId],
                    ExpiryDate = expiry.TryGetValue(item.ProductId, out var expiryDate) ? expiryDate : null
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
            await transaction.CommitAsync();
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackex)
            {
                ModelState.AddModelError("", rollbackex.Message);
            }

            ModelState.AddModelError("", ex.Message);
            await PopulateSaleDropdown(svm);
            return View(svm);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await context.Sales
            .AsNoTracking()
            .Include(s => s.CreatedByUser)
            .Include(sale => sale.SaleLineItems)
            .ThenInclude(saleLineItem => saleLineItem.Product)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (model == null) return NotFound();
        return View(new SaleViewModel
        {
            Id = model.Id,
            SaleDate = model.SaleDate,
            TotalAmount = model.TotalAmount,
            CreatedByUserName = model.CreatedByUser?.UserName,
            SaleLineItems = model.SaleLineItems.Select(sl => new SaleLineItemViewModel
            {
                Id = sl.Id,
                SaleId = sl.SaleId,
                ProductName = sl.Product.Name,
                ProductId = sl.ProductId,
                Quantity = sl.Quantity,
                UnitPrice = sl.UnitPrice,
                TotalPrice = sl.TotalPrice
            })
        });
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteSaleConfirmed(int id)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var sale = await context.Sales.Include(sale => sale.SaleLineItems)
            .ThenInclude(saleLineItem => saleLineItem.Product).Include(sale => sale.CreatedByUser)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (sale == null) return NotFound();
        try
        {
            var saleItems = sale.SaleLineItems.Select(sl => new
                { Id = sl.ProductId, Quant = sl.Quantity, Expiry = sl.ExpiryDate });
            foreach (var saleItem in saleItems)
            {
                var shipment = await context.Shipments
                    .FirstOrDefaultAsync(sh => sh.ExpiryDate == saleItem.Expiry && sh.ProductId == saleItem.Id);

                if (shipment == null)
                {
                    context.Shipments.Add(new Shipment
                    {
                        ProductId = saleItem.Id,
                        Quantity = saleItem.Quant,
                        ExpiryDate = saleItem.Expiry,
                        ReceivedDate = DateTime.Now
                    });
                }
                else
                {
                    shipment.Quantity += saleItem.Quant;
                }

            }

            context.Sales.Remove(sale);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return RedirectToAction("Index");
        }
        catch (DbUpdateException ex)
        {
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackex)
            {
                ModelState.AddModelError("", rollbackex.Message);
            }

            ModelState.AddModelError("", ex.Message);
            return View(new SaleViewModel
            {
                Id = sale.Id,
                SaleDate = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                CreatedByUserName = sale.CreatedByUser?.UserName,
                SaleLineItems = sale.SaleLineItems.Select(sl => new SaleLineItemViewModel
                {
                    Id = sl.Id,
                    SaleId = sl.SaleId,
                    ProductName = sl.Product.Name,
                    ProductId = sl.ProductId,
                    Quantity = sl.Quantity,
                    UnitPrice = sl.UnitPrice,
                    TotalPrice = sl.TotalPrice
                })
            });
        }
    }
    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var sale = await context.Sales
            .Include(sale => sale.CreatedByUser)
            .Include(sale => sale.SaleLineItems)
            .ThenInclude(saleLineItem => saleLineItem.Product)
            .FirstOrDefaultAsync(s => s.Id == id);
        var products = context.Products;
        if (sale == null) return NotFound();
        var model = new SaleViewModel
        {
            Id = sale.Id,
            SaleDate = sale.SaleDate,
            TotalAmount = sale.TotalAmount,
            CreatedByUserName = sale.CreatedByUser?.UserName,
            SaleLineItems = sale.SaleLineItems.Select(sli => new SaleLineItemViewModel
            {
                Id = sli.Id,
                SaleId = sli.SaleId,
                ProductName = sli.Product.Name,
                ProductId = sli.ProductId,
                Quantity = sli.Quantity,
                UnitPrice = sli.UnitPrice,
                TotalPrice = sli.TotalPrice,
                Products = products.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList()
            })
        };
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(SaleViewModel svm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSaleDropdown(svm);
            return View(svm);
        }

        var sale = await context.Sales
            .Include(sale => sale.SaleLineItems)
            .FirstOrDefaultAsync(s => s.Id == svm.Id);
        if (sale == null) return NotFound();
        try
        {
            var itemsToRemove = sale.SaleLineItems
                .Where(item => svm.SaleLineItems.All(sli => sli.Id != item.Id || sli.Quantity == 0))
                .ToList();

            foreach (var item in itemsToRemove)
            {
                RegulateShipments(item.ProductId,item.Quantity,item.ExpiryDate);
                sale.SaleLineItems.Remove(item);
            }

            foreach (var item in sale.SaleLineItems)
            {
                var modelItem = svm.SaleLineItems.FirstOrDefault(sl => sl.Id == item.Id);
                if (modelItem == null) continue;

                if (item.ProductId != modelItem.ProductId)
                {
                    try
                    {
                        RegulateShipments(modelItem.ProductId, -modelItem.Quantity); // Deduct new product's stock
                        RegulateShipments(item.ProductId, item.Quantity,item.ExpiryDate); // Return old product's stock
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", $"Not enough stock for product ID change to {modelItem.ProductId}");
                        await PopulateSaleDropdown(svm);
                        return View(svm);
                    }
                    item.ProductId = modelItem.ProductId;
                    item.Quantity = modelItem.Quantity;
                    item.ExpiryDate = context.Shipments
                        .Where(sh => sh.ProductId == modelItem.ProductId)
                        .OrderBy(sh => sh.ExpiryDate)
                        .Select(sh => sh.ExpiryDate)
                        .FirstOrDefault();
                }
                else
                {
                    int quantityDelta = modelItem.Quantity - item.Quantity;
                    try
                    {
                        RegulateShipments(item.ProductId, quantityDelta,item.ExpiryDate);
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("",$"Not enough stock for product ID {item.ProductId}");
                        await PopulateSaleDropdown(svm);
                        return View(svm);
                    }
                    item.Quantity = modelItem.Quantity;
                }
                
                item.UnitPrice = modelItem.UnitPrice;
            }

            sale.TotalAmount = sale.SaleLineItems.Sum(s => s.TotalPrice);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            ModelState.AddModelError("", e.Message);
            await PopulateSaleDropdown(svm);
            return View(svm);
        }
    }

    private async Task PopulateSaleDropdown(SaleViewModel model)
    {

        var products = await context.Products.ToListAsync();
        ViewBag.ProductPrices = products.ToDictionary(p => p.Id, p => p.Price);
        if (model.SaleLineItems == null || !model.SaleLineItems.Any())
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

    private void RegulateShipments(int productId, int quantityChange,DateTime? expiry=null)
    {
        var shipments = context.Shipments
            .Where(sh => sh.ProductId == productId)
            .OrderBy(sh => sh.ExpiryDate)
            .ToList();

        if (quantityChange == 0) return;

        if (quantityChange < 0) // Deduct stock
        {
            int toDeduct = -quantityChange;

            int available = shipments.Sum(s => s.Quantity);
            if (available < toDeduct)
                throw new Exception();

            foreach (var shipment in shipments)
            {
                if (toDeduct == 0) break;

                int used = Math.Min(shipment.Quantity, toDeduct);
                shipment.Quantity -= used;
                toDeduct -= used;

                if (shipment.Quantity == 0)
                    context.Shipments.Remove(shipment);
            }
        }
        else // Add stock
        {
            var addingShipment = shipments.FirstOrDefault(sh => sh.ExpiryDate == expiry);
            if (addingShipment == null)
            {
                context.Shipments.Add(new Shipment
                {
                    ProductId = productId,
                    Quantity = quantityChange,
                    ExpiryDate = expiry
                });
            }
            else
            {
                addingShipment.Quantity += quantityChange;
            }

        }
    }
}