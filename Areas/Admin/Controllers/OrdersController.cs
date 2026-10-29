using AppleStore.Data;
using AppleStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;

    public OrdersController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: Admin/Orders?status=Pending&page=1
    public async Task<IActionResult> Index(string? status, int page = 1)
    {
        var query = _db.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, out var parsed))
        {
            query = query.Where(o => o.Status == parsed);
        }

        ViewBag.Status = status;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / 15.0);
        ViewBag.StatusCounts = await _db.Orders.AsNoTracking()
            .GroupBy(o => o.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((Math.Max(page, 1) - 1) * 15)
            .Take(15)
            .Select(o => new Order
            {
                Id = o.Id,
                FullName = o.FullName,
                Email = o.Email,
                OrderDate = o.OrderDate,
                Status = o.Status,
                Total = o.Total,
                OrderDetails = o.OrderDetails
            })
            .ToListAsync();

        return View(orders);
    }

    // GET: Admin/Orders/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.OrderDetails)
            .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        return View(order);
    }

    // POST: Admin/Orders/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        var oldStatus = order.Status;
        order.Status = status;

        // Nếu admin huỷ đơn — hoàn lại tồn kho
        if (status == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
        {
            var details = await _db.OrderDetails
                .Where(d => d.OrderId == id)
                .ToListAsync();

            foreach (var detail in details)
            {
                var product = await _db.Products.FindAsync(detail.ProductId);
                if (product != null)
                    product.Stock += detail.Quantity;
            }
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã cập nhật đơn #{id} → {status.GetDisplayName()}.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
