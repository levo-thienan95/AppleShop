using AppleStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Components;

/// <summary>Thanh danh mục trong navbar — tự động hiển thị danh mục từ database.</summary>
public class CategoriesNavViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public CategoriesNavViewComponent(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }
}
