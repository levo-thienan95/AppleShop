using System.ComponentModel.DataAnnotations;

namespace AppleStore.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [StringLength(100)]
    [Display(Name = "Tên danh mục")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [StringLength(300)]
    [Display(Name = "Ảnh danh mục")]
    public string? ImageUrl { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
