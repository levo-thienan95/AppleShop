using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppleStore.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
    [StringLength(200)]
    [Display(Name = "Tên sản phẩm")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Mô tả ngắn")]
    public string? ShortDescription { get; set; }

    [Display(Name = "Mô tả chi tiết")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Giá bán là bắt buộc")]
    [Column(TypeName = "decimal(18,0)")]
    [Range(0, 1000000000, ErrorMessage = "Giá bán không hợp lệ")]
    [Display(Name = "Giá bán (VNĐ)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    [Display(Name = "Giá cũ (VNĐ)")]
    public decimal? OldPrice { get; set; }

    [Required(ErrorMessage = "Ảnh sản phẩm là bắt buộc")]
    [StringLength(300)]
    [Display(Name = "Ảnh sản phẩm")]
    public string ImageUrl { get; set; } = string.Empty;

    [Display(Name = "Nổi bật")]
    public bool IsFeatured { get; set; }

    [Range(0, 1000000, ErrorMessage = "Số lượng tồn kho không hợp lệ")]
    [Display(Name = "Tồn kho")]
    public int Stock { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
