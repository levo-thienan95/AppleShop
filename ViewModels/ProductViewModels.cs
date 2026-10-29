using AppleStore.Models;

namespace AppleStore.ViewModels;

/// <summary>Danh sách sản phẩm có lọc danh mục, tìm kiếm và phân trang.</summary>
public class ProductCatalogViewModel
{
    public IReadOnlyList<Product> Products { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();

    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string SortBy { get; set; } = "new"; // new | price-asc | price-desc

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

/// <summary>Trang chi tiết sản phẩm: sản phẩm + gợi ý liên quan.</summary>
public class ProductDetailViewModel
{
    public Product Product { get; set; } = null!;
    public IReadOnlyList<Product> RelatedProducts { get; set; } = Array.Empty<Product>();
}

/// <summary>Dữ liệu cho trang chủ.</summary>
public class HomeViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Product> FeaturedProducts { get; set; } = new();
    public List<Product> NewProducts { get; set; } = new();
    public List<Product> BestSellingProducts { get; set; } = new();
}
