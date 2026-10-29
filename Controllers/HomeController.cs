using System.Diagnostics;
using AppleStore.Data;
using AppleStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppleStore.ViewModels;

namespace AppleStore.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>Trang chủ: banner danh mục + sản phẩm nổi bật + hàng mới về.</summary>
    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel
        {
            Categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(),

            FeaturedProducts = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.IsFeatured)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync(),

            NewProducts = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync(),

            BestSellingProducts = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .OrderByDescending(p => p.Price)
                .Take(4)
                .ToListAsync()
        };

        return View(model);
    }

    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None, Duration = 0)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
