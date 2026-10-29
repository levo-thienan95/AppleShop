using AppleStore.Data;
using AppleStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _db;

    public CategoriesController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .Select(c => new Category
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                Products = c.Products
            })
            .ToListAsync();

        // Đếm số sản phẩm mỗi danh mục
        var counts = await _db.Products.AsNoTracking()
            .GroupBy(p => p.CategoryId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        ViewBag.Counts = counts;
        return View(categories);
    }

    [HttpGet]
    public IActionResult Create() => View(new Category());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (await _db.Categories.AnyAsync(c => c.Name == category.Name))
        {
            ModelState.AddModelError("Name", "Danh mục đã tồn tại.");
        }

        if (!ModelState.IsValid) return View(category);

        category.Slug = GenerateSlug(category.Name);
        category.ImageUrl ??= "/images/placeholder.svg";
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã thêm danh mục \"{category.Name}\".";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id) return BadRequest();
        if (!ModelState.IsValid) return View(category);

        var current = await _db.Categories.FindAsync(id);
        if (current == null) return NotFound();

        current.Name = category.Name;
        current.Description = category.Description;
        current.ImageUrl = string.IsNullOrWhiteSpace(category.ImageUrl)
            ? current.ImageUrl
            : category.ImageUrl;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã cập nhật danh mục \"{category.Name}\".";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();

        var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            TempData["Error"] = $"Không thể xoá \"{category.Name}\" vì còn sản phẩm thuộc danh mục này.";
            return RedirectToAction(nameof(Index));
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã xoá danh mục \"{category.Name}\".";
        return RedirectToAction(nameof(Index));
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
