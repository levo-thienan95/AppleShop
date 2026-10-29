using AppleStore.Data;
using AppleStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProductsController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // GET: Admin/Products?q=...&page=1
    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = _db.Products.AsNoTracking().Include(p => p.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim();
            query = query.Where(p => p.Name.Contains(kw));
        }

        ViewBag.q = q;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / 12.0);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((Math.Max(page, 1) - 1) * 12)
            .Take(12)
            .ToListAsync();

        return View(items);
    }

    // GET: Admin/Products/Create
    public async Task<IActionResult> Create()
    {
        await LoadCategories();
        return View(new Product { CreatedAt = DateTime.Now });
    }

    // POST: Admin/Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
    {
        if (!string.IsNullOrWhiteSpace(product.Name) &&
            await _db.Products.AnyAsync(p => p.Slug == product.Slug))
        {
            ModelState.AddModelError("Slug", "Slug đã tồn tại, hãy dùng tên khác.");
        }

        if (await ApplyImageAsync(product, imageFile) is string imageError)
        {
            ModelState.AddModelError("ImageUrl", imageError);
        }

        if (!ModelState.IsValid)
        {
            await LoadCategories();
            return View(product);
        }

        product.Slug = string.IsNullOrWhiteSpace(product.Slug)
            ? GenerateSlug(product.Name)
            : GenerateSlug(product.Slug);
        product.CreatedAt = DateTime.Now;

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã thêm sản phẩm \"{product.Name}\".";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Products/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        await LoadCategories();
        return View(product);
    }

    // POST: Admin/Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
    {
        if (id != product.Id) return BadRequest();

        if (await ApplyImageAsync(product, imageFile) is string imageError)
        {
            ModelState.AddModelError("ImageUrl", imageError);
        }

        if (!ModelState.IsValid)
        {
            await LoadCategories();
            return View(product);
        }

        var current = await _db.Products.FindAsync(id);
        if (current == null) return NotFound();

        current.Name = product.Name;
        current.Slug = string.IsNullOrWhiteSpace(product.Slug)
            ? current.Slug
            : GenerateSlug(product.Slug);
        current.ShortDescription = product.ShortDescription;
        current.Description = product.Description;
        current.Price = product.Price;
        current.OldPrice = product.OldPrice;
        current.Stock = product.Stock;
        current.IsFeatured = product.IsFeatured;
        current.CategoryId = product.CategoryId;
        current.ImageUrl = product.ImageUrl;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã cập nhật \"{product.Name}\".";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Products/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.AsNoTracking().Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();
        return View(product);
    }

    // POST: Admin/Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        try
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã xoá \"{product.Name}\".";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xoá: sản phẩm đã xuất hiện trong đơn hàng của khách.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategories()
    {
        ViewBag.Categories = new SelectList(
            await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name");
    }

    /// <summary>Xử lý upload ảnh (nếu có file) — trả về thông báo lỗi hoặc null.</summary>
    private async Task<string?> ApplyImageAsync(Product product, IFormFile? imageFile)
    {
        if (imageFile is { Length: > 0 })
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                return "Chỉ chấp nhận ảnh .jpg, .jpeg, .png, .webp, .gif.";

            if (imageFile.Length > 2 * 1024 * 1024)
                return "Ảnh tối đa 2MB.";

            var fileName = $"{GenerateSlug(Path.GetFileNameWithoutExtension(imageFile.FileName))}-{Guid.NewGuid():N}{ext}";
            var folder = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(folder);

            await using var stream = System.IO.File.Create(Path.Combine(folder, fileName));
            await imageFile.CopyToAsync(stream);

            product.ImageUrl = $"/images/products/{fileName}";
        }

        if (string.IsNullOrWhiteSpace(product.ImageUrl))
            product.ImageUrl = "/images/placeholder.svg";

        return null;
    }

    private static string GenerateSlug(string value)
    {
        var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var ch in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch)
                == System.Globalization.UnicodeCategory.NonSpacingMark)
                continue;

            sb.Append(char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '-');
        }

        var slug = sb.ToString().Trim('-');
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");
        return slug;
    }
}
