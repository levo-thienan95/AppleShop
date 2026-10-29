using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AppleStore.Models;

public enum OrderStatus
{
    [Display(Name = "Chờ xác nhận")]
    Pending = 0,
    [Display(Name = "Đang xử lý")]
    Processing = 1,
    [Display(Name = "Đang giao")]
    Shipped = 2,
    [Display(Name = "Hoàn thành")]
    Completed = 3,
    [Display(Name = "Đã hủy")]
    Cancelled = 4
}

public class Order
{
    public int Id { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    [Required(ErrorMessage = "Họ tên người nhận là bắt buộc")]
    [StringLength(200)]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(200)]
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

    [Display(Name = "Ngày đặt")]
    public DateTime OrderDate { get; set; } = DateTime.Now;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Column(TypeName = "decimal(18,0)")]
    public decimal Total { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

public class OrderDetail
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Range(1, 1000000)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal UnitPrice { get; set; }
}
