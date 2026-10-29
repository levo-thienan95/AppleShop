using AppleStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>Tổng quan: doanh thu, đơn hàng, tồn kho, đơn gần đây.</summary>
    public async Task<IActionResult> Index()
    {
        var validOrders = _db.Orders.AsNoTracking().Where(o => o.Status != OrderStatusCancelled());

        ViewBag.Revenue = await validOrders.SumAsync(o => (decimal?)o.Total) ?? 0m;
        ViewBag.OrderCount = await _db.Orders.AsNoTracking().CountAsync();
        ViewBag.PendingCount = await _db.Orders.AsNoTracking().CountAsync(o => o.Status == Models.OrderStatus.Pending);
        ViewBag.ProductCount = await _db.Products.AsNoTracking().CountAsync();
        ViewBag.UserCount = await _db.Users.AsNoTracking().CountAsync();

        ViewBag.RecentOrders = await _db.Orders.AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Take(6)
            .Select(o => new { o.Id, o.FullName, o.OrderDate, o.Total, o.Status })
            .ToListAsync();

        ViewBag.LowStock = await _db.Products.AsNoTracking()
            .Where(p => p.Stock <= 10)
            .OrderBy(p => p.Stock)
            .Take(6)
            .Select(p => new { p.Id, p.Name, p.Stock, p.ImageUrl })
            .ToListAsync();

        // Top sản phẩm theo doanh thu
        ViewBag.TopProducts = await _db.OrderDetails.AsNoTracking()
            .GroupBy(d => new { d.ProductId, d.Product.Name })
            .Select(g => new
            {
                g.Key.Name,
                Quantity = g.Sum(d => d.Quantity),
                Revenue = g.Sum(d => d.UnitPrice * d.Quantity)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync();

        return View();
    }

    private static Models.OrderStatus OrderStatusCancelled() => Models.OrderStatus.Cancelled;
}
