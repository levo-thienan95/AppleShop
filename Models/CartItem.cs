using System.ComponentModel.DataAnnotations;

namespace AppleStore.Models;

/// <summary>Một dòng trong giỏ hàng (lưu ở Session, không cần bảng DB).</summary>
public class CartItem
{
    public int ProductId { get; set; }

    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal Subtotal => Price * Quantity;
}

/// <summary>Thông tin giao hàng khi đặt đơn.</summary>
public class CheckoutViewModel
{
    [Required(ErrorMessage = "Họ tên người nhận là bắt buộc")]
    [StringLength(200)]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20)]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
    [StringLength(500)]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Tỉnh / Thành phố")]
    public string? City { get; set; }

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }
}
