using AppleStore.Data;
using AppleStore.Models;
using AppleStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Controllers;

/// <summary>Giỏ hàng (Session) và đặt hàng (Checkout).</summary>
[Authorize]
public class CartController : Controller
{
    private readonly ApplicationDbContext _db;

    public CartController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>Trang giỏ hàng.</summary>
    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetCart();
        return View(cart);
    }

    /// <summary>Thêm sản phẩm vào giỏ (POST từ card sản phẩm hoặc trang chi tiết).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        var product = await _db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null)
            return NotFound();

        if (product.Stock <= 0)
        {
            TempData["Error"] = $"Sản phẩm \"{product.Name}\" hiện đã hết hàng.";
            return Redirect(returnUrl ?? Url.Action("Index", "Products")!);
        }

        quantity = Math.Clamp(quantity, 1, Math.Max(product.Stock, 1));

        var cart = HttpContext.Session.GetCart();
        var item = cart.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
        {
            cart.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                Quantity = quantity
            });
        }
        else
        {
            item.Quantity = Math.Clamp(item.Quantity + quantity, 1, Math.Max(product.Stock, 1));
        }

        HttpContext.Session.SaveCart(cart);
        TempData["Success"] = $"Đã thêm \"{product.Name}\" vào giỏ hàng.";

        return Redirect(returnUrl ?? Url.Action("Index", "Cart")!);
    }

    /// <summary>Cập nhật số lượng.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int productId, int quantity)
    {
        var cart = HttpContext.Session.GetCart();
        var item = cart.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
                cart.Remove(item);
            else
                item.Quantity = quantity;
            HttpContext.Session.SaveCart(cart);
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Xoá một dòng khỏi giỏ.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        var cart = HttpContext.Session.GetCart();
        cart.RemoveAll(i => i.ProductId == productId);
        HttpContext.Session.SaveCart(cart);
        TempData["Success"] = "Đã xoá sản phẩm khỏi giỏ hàng.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Trang xác nhận đơn hàng — chỉ thành viên đã đăng nhập.</summary>
    public async Task<IActionResult> Checkout()
    {
        var cart = HttpContext.Session.GetCart();
        if (cart.Count == 0)
        {
            TempData["Error"] = "Giỏ hàng đang trống.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _db.Users.FindAsync(UserManager().GetUserId(User));
        var model = new CheckoutViewModel
        {
            FullName = user?.FullName ?? string.Empty,
            Email = user?.Email ?? string.Empty,
            Phone = user?.PhoneNumber ?? string.Empty,
            Address = user?.Address ?? string.Empty
        };

        ViewBag.Cart = cart;
        ViewBag.CartTotal = cart.Sum(i => i.Subtotal);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var cart = HttpContext.Session.GetCart();
        if (cart.Count == 0)
        {
            TempData["Error"] = "Giỏ hàng đang trống.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Cart = cart;
            ViewBag.CartTotal = cart.Sum(i => i.Subtotal);
            return View(model);
        }

        // Kiểm tra tồn kho theo giá hiện tại trong DB
        var productIds = cart.Select(i => i.ProductId).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var item in cart)
        {
            if (!products.TryGetValue(item.ProductId, out var p))
            {
                TempData["Error"] = "Một sản phẩm trong giỏ hàng không còn tồn tại.";
                return RedirectToAction(nameof(Index));
            }
            if (p.Stock < item.Quantity)
            {
                TempData["Error"] = $"Sản phẩm \"{p.Name}\" chỉ còn {p.Stock} chiếc trong kho.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Tạo đơn hàng
        var userId = UserManager().GetUserId(User);
        var order = new Order
        {
            UserId = userId,
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Phone = model.Phone.Trim(),
            Address = model.Address.Trim(),
            City = model.City?.Trim(),
            Note = model.Note?.Trim(),
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderDetails = cart.Select(item => new OrderDetail
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = products[item.ProductId].Price // giá tại thời điểm đặt
            }).ToList()
        };
        order.Total = order.OrderDetails.Sum(d => d.UnitPrice * d.Quantity);

        // Trừ tồn kho
        foreach (var detail in order.OrderDetails)
        {
            products[detail.ProductId].Stock -= detail.Quantity;
        }

        // Cập nhật profile người dùng cho lần mua sau
        var user = await _db.Users.FindAsync(userId);
        if (user != null)
        {
            user.FullName = model.FullName.Trim();
            user.Address = model.Address.Trim();
            user.PhoneNumber = model.Phone.Trim();
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        HttpContext.Session.ClearCart();
        TempData["Success"] = $"Đặt hàng thành công! Mã đơn #{order.Id}. Cảm ơn bạn đã mua sắm.";
        return RedirectToAction("Details", "Orders", new { id = order.Id });
    }

    private Microsoft.AspNetCore.Identity.UserManager<Models.ApplicationUser> UserManager()
        => HttpContext.RequestServices
            .GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Models.ApplicationUser>>();
}
