using InventoryManagment.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagment.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Product>Products { get; set; }
    public DbSet<Shipment>Shipments { get; set; }
    public DbSet<Category>Categories { get; set; }
    public DbSet<Sale>Sales { get; set; }
    public DbSet<SaleLineItem>SaleLineItems { get; set; }
    public DbSet<Supplier>Suppliers { get; set; }
}