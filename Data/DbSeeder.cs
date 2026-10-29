using AppleStore.Models;
using Microsoft.AspNetCore.Identity;

namespace AppleStore.Data;

/// <summary>Seed dữ liệu ban đầu: Role Admin, tài khoản Admin, danh mục và sản phẩm Apple.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        // 1) Role Admin
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // 2) Tài khoản Admin
        var adminEmail = "admin@applestore.vn";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Quản trị viên"
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        // 3) Danh mục + sản phẩm (chỉ seed 1 lần)
        if (db.Categories.Any() || db.Products.Any()) return;

        var categories = new List<Category>
        {
            new() { Name = "iPhone",       Slug = "iphone",       ImageUrl = "/images/categories/iphone.jpg",
                     Description = "Điện thoại iPhone chính hãng — từ iPhone 14 đến iPhone 16 Pro Max." },
            new() { Name = "iPad",         Slug = "ipad",         ImageUrl = "/images/categories/ipad.jpg",
                     Description = "Máy tính bảng iPad cho học tập, sáng tạo và giải trí." },
            new() { Name = "Mac",          Slug = "mac",          ImageUrl = "/images/categories/mac.jpg",
                     Description = "MacBook, iMac và Mac mini với chip M-series mạnh mẽ." },
            new() { Name = "Apple Watch",  Slug = "apple-watch",  ImageUrl = "/images/categories/apple-watch.jpg",
                     Description = "Đồng hồ thông minh theo dõi sức khỏe và thể thao." },
            new() { Name = "AirPods",      Slug = "airpods",      ImageUrl = "/images/categories/airpods.jpg",
                     Description = "Tai nghe không dây AirPods với âm thanh chất lượng cao." },
            new() { Name = "Phụ kiện",     Slug = "phu-kien",     ImageUrl = "/images/categories/phu-kien.jpg",
                     Description = "Sạc, cáp, chuột, bàn phím và phụ kiện Apple chính hãng." },
        };
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        var cat = categories.ToDictionary(c => c.Slug, c => c);

        // Hàm trợ giúp tạo Product — tự gán Category theo slug
        Product P(
            string slug, string name, string categorySlug, decimal price, decimal? oldPrice,
            bool featured, int stock, string shortDescription, string description)
        {
            var category = cat[categorySlug];
            return new Product
            {
                Slug = slug,
                Name = name,
                Price = price,
                OldPrice = oldPrice,
                IsFeatured = featured,
                Stock = stock,
                ShortDescription = shortDescription,
                Description = description,
                ImageUrl = $"/images/products/{slug}.jpg",
                CategoryId = category.Id,
                Category = category
            };
        }

        var products = new List<Product>
        {
            // iPhone
            P("iphone-16-pro-max-256gb", "iPhone 16 Pro Max 256GB", "iphone", 34_990_000, 36_990_000, true, 25,
              "Màn hình 6,9 inch Super Retina XDR, chip A18 Pro, camera 48MP, khung titan.",
              "iPhone 16 Pro Max — chiếc iPhone mạnh mẽ nhất từ trước đến nay với chip A18 Pro, "
            + "hệ thống Camera Control và thời lượng pin vượt trội."),
            P("iphone-16-pro-128gb", "iPhone 16 Pro 128GB", "iphone", 27_990_000, null, true, 30,
              "Màn hình 6,3 inch, chip A18 Pro, khung titan, Camera Control.",
              "iPhone 16 Pro mang khung titan đẳng cấp, khả năng quay 4K 120fps và "
            + "tính năng Camera Control giúp chụp ảnh nhanh hơn bao giờ hết."),
            P("iphone-16-128gb", "iPhone 16 128GB", "iphone", 19_990_000, 22_990_000, true, 40,
              "Chip A18, camera Control, 5 màu sắc trẻ trung.",
              "iPhone 16 với chip A18 thế hệ mới, nút Camera Control và thiết kế khung phẳng hiện đại."),
            P("iphone-16-plus-128gb", "iPhone 16 Plus 128GB", "iphone", 22_990_000, null, false, 28,
              "Màn hình 6,7 inch, pin cả ngày, chip A18.",
              "iPhone 16 Plus cho thời lượng pin cả ngày với màn hình Super Retina XDR 6,7 inch."),
            P("iphone-15-128gb", "iPhone 15 128GB", "iphone", 15_990_000, 19_990_000, false, 35,
              "Dynamic Island, camera 48MP, cổng USB-C.",
              "iPhone 15 với Dynamic Island, camera chính 48MP và cổng USB-C tiện lợi — "
            + "giá tốt nhất phân khúc cao cấp."),
            P("iphone-14-128gb", "iPhone 14 128GB", "iphone", 12_990_000, null, false, 20,
              "Chip A15 Bionic, camera kép 12MP, màn hình 6,1 inch.",
              "iPhone 14 — lựa chọn phổ thông đáng mua với hiệu năng A15 Bionic còn rất mạnh."),

            // iPad
            P("ipad-pro-m4-11-256gb", "iPad Pro M4 11 inch 256GB Wi-Fi", "ipad", 27_990_000, null, true, 15,
              "Chip M4, màn hình Tandem OLED, mỏng 5,1mm.",
              "iPad Pro M4 mỏng nhất lịch sử Apple với màn hình Tandem OLED rực rỡ và "
            + "hiệu năng M4 phục vụ sáng tạo chuyên nghiệp."),
            P("ipad-air-m2-11-128gb", "iPad Air M2 11 inch 128GB", "ipad", 15_990_000, null, false, 22,
              "Chip M2, hỗ trợ Apple Pencil Pro, 4 màu.",
              "iPad Air M2 cân bằng giữa hiệu năng và giá cả, hỗ trợ Apple Pencil Pro cho "
            + "ghi chú và vẽ vời tuyệt vời."),
            P("ipad-a16-11-128gb", "iPad A16 11 inch 128GB", "ipad", 9_990_000, 10_990_000, true, 30,
              "Chip A16 Bionic, màn hình 11 inch, giá tốt cho sinh viên.",
              "iPad thế hệ mới với chip A16 — chiếc máy tính bảng tốt nhất cho học tập "
            + "và giải trí trong tầm giá 10 triệu."),
            P("ipad-mini-a17-128gb", "iPad mini A17 Pro 128GB", "ipad", 13_990_000, null, false, 18,
              "Màn hình 8,3 inch, chip A17 Pro, hỗ trợ Apple Intelligence.",
              "iPad mini A17 Pro nhỏ gọn nhưng mạnh mẽ, dễ mang theo mọi lúc mọi nơi."),

            // Mac
            P("macbook-air-m3-13-256gb", "MacBook Air M3 13 inch 8GB/256GB", "mac", 24_990_000, 27_990_000, true, 20,
              "Chip M3, thiết kế không quạt, pin 18 giờ.",
              "MacBook Air M3 — chiếc laptop mỏng nhẹ best-seller, chạy êm không quạt, "
            + "pin trụ được cả ngày làm việc."),
            P("macbook-pro-m4-14-512gb", "MacBook Pro M4 14 inch 16GB/512GB", "mac", 44_990_000, null, true, 12,
              "Chip M4, màn hình Liquid Retina XDR, cổng Thunderbolt 5.",
              "MacBook Pro M4 với màn hình XDR chuyên nghiệp, hiệu năng đỉnh cao cho "
            + "lập trình, thiết kế và dựng phim."),
            P("macbook-pro-m4-pro-16", "MacBook Pro M4 Pro 16 inch 24GB/512GB", "mac", 67_990_000, null, false, 8,
              "Chip M4 Pro, màn hình 16 inch, 24GB RAM.",
              "MacBook Pro 16 inch M4 Pro — trạm làm việc di động cho chuyên gia."),
            P("imac-m4-24-256gb", "iMac M4 24 inch 256GB", "mac", 34_990_000, null, false, 10,
              "Chip M4, màn hình 24 inch Retina 4.5K, 4 màu.",
              "iMac M4 all-in-one màu sắc rực rỡ, tiết kiệm không gian bàn làm việc."),
            P("mac-mini-m4-256gb", "Mac mini M4 256GB", "mac", 16_990_000, null, false, 16,
              "Chip M4, kích thước nhỏ gọn, cổng USB-C phía trước.",
              "Mac mini M4 nhỏ gọn mạnh mẽ — máy tính để bàn Apple giá dễ tiếp cận nhất."),

            // Apple Watch
            P("apple-watch-series-10-46mm", "Apple Watch Series 10 46mm GPS", "apple-watch", 11_990_000, null, true, 24,
              "Màn hình lớn nhất, chip S10, theo dõi giấc ngủ và thở ngừng thở.",
              "Apple Watch Series 10 với màn hình rộng nhất từ trước đến nay, "
            + "sạc nhanh 30 phút đủ dùng cả ngày."),
            P("apple-watch-ultra-2-49mm", "Apple Watch Ultra 2 49mm", "apple-watch", 22_990_000, null, false, 10,
              "Vỏ titan, độ bền chuẩn quân đội, GPS hai tần.",
              "Apple Watch Ultra 2 — dành cho phiêu lưu với độ bền khủng và độ sáng 3000 nits."),
            P("apple-watch-se-40mm", "Apple Watch SE 40mm GPS", "apple-watch", 6_490_000, 7_490_000, false, 26,
              "Giá tốt, đủ tính năng sức khỏe cốt lõi.",
              "Apple Watch SE — bước đầu hợp lý nhất vào thế giới đồng hồ thông minh Apple."),

            // AirPods
            P("airpods-pro-2-usbc", "AirPods Pro 2 USB-C", "airpods", 5_490_000, 6_290_000, true, 40,
              "Chống ồn chủ động, Adaptive Audio, hộp sạc USB-C.",
              "AirPods Pro 2 USB-C với khả năng chống ồn chủ động xuất sắc và "
            + "âm thanh không gian cá nhân hóa."),
            P("airpods-4", "AirPods 4", "airpods", 3_490_000, null, true, 45,
              "Thiết kế thoải mái, chip H2, âm thanh không gian.",
              "AirPods 4 — tai nghe In-ear không stem silicone, nhẹ và thoải mái cả ngày."),
            P("airpods-max-usbc", "AirPods Max USB-C", "airpods", 14_990_000, null, false, 12,
              "Over-ear, chống ồn, âm thanh không gian Lossless.",
              "AirPods Max — tai nghe over-ear cao cấp với chất âm chi tiết và "
            + "thiết kế nhôm sang trọng."),

            // Phụ kiện
            P("magsafe-charger", "MagSafe Charger", "phu-kien", 990_000, null, false, 60,
              "Sạc không dây từ tính 25W (với iPhone 16 Pro).",
              "MagSafe Charger gắn chắc vào mặt sau iPhone, sạc không dây nhanh gọn."),
            P("magic-mouse", "Apple Magic Mouse", "phu-kien", 2_490_000, null, false, 30,
              "Chuột không dây, sạc USB-C, cảm ứng đa điểm.",
              "Magic Mouse với bề mặt cảm ứng đa điểm và kết nối Bluetooth liền mạch."),
            P("apple-30w-usb-c-adapter", "Apple 30W USB-C Power Adapter", "phu-kien", 1_190_000, null, false, 50,
              "Sạc nhanh USB-C cho iPhone, iPad và MacBook Air.",
              "Bộ sạc Apple 30W chính hãng, an toàn và tương thích rộng."),
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }

}
