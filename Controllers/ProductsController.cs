using AppleStore.Data;
using AppleStore.Models;
using AppleStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Controllers;

/// <summary>Trang danh sách sản phẩm (lọc danh mục, tìm kiếm, sắp xếp, phân trang)
/// và trang chi tiết sản phẩm.</summary>
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProductsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /Products?categoryId=2&q=iphone&page=2&sort=price-asc
    public async Task<IActionResult> Index(int? categoryId, string? q, string? sort, int page = 1)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var keyword = q.Trim();
            query = query.Where(p =>
                p.Name.Contains(keyword) ||
                (p.ShortDescription != null && p.ShortDescription.Contains(keyword)) ||
                (p.Description != null && p.Description.Contains(keyword)));
        }

        query = sort switch
        {
            "price-asc" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var model = new ProductCatalogViewModel
        {
            Products = await query
                .Skip((Math.Max(page, 1) - 1) * 12)
                .Take(12)
                .ToListAsync(),
            Categories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(),
            Search = q,
            CategoryId = categoryId,
            CategoryName = categoryId.HasValue
                ? await _db.Categories.AsNoTracking()
                    .Where(c => c.Id == categoryId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync()
                : null,
            SortBy = sort ?? "new",
            Page = Math.Max(page, 1),
            TotalCount = totalCount
        };

        return View(model);
    }

    // GET: /Products/Details/iphone-16-pro-max-256gb  (theo slug) hoặc ?id=5
    public async Task<IActionResult> Details(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return RedirectToAction(nameof(Index));

        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == id || p.Id.ToString() == id);

        if (product == null)
            return NotFound();

        var related = await _db.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
            .OrderByDescending(p => p.IsFeatured)
            .Take(4)
            .ToListAsync();

        return View(new ProductDetailViewModel
        {
            Product = product,
            RelatedProducts = related
        });
    }
}
