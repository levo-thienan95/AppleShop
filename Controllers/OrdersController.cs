using AppleStore.Data;
using AppleStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Controllers;

/// <summary>Lịch sử đặt hàng của thành viên.</summary>
[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;

    public OrdersController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>Danh sách đơn của tôi.</summary>
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.OrderDetails)
            .ThenInclude(d => d.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    /// <summary>Chi tiết một đơn — chỉ chủ đơn hoặc Admin xem được.</summary>
    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.OrderDetails)
            .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && order.UserId != userId)
            return Forbid();

        return View(order);
    }
}
