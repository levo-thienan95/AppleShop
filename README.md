# AppleStore — Đồ án ASP.NET Core MVC

Website thương mại điện tử bán sản phẩm Apple, xây dựng bằng **ASP.NET Core 8 MVC**,
**ASP.NET Core Identity**, **Entity Framework Core** và **SQL Server**.

---

## 1. Tính năng

### Khu vực khách hàng
- Trang chủ: banner khuyến mãi, danh mục nhanh, sản phẩm nổi bật, hàng mới về.
- Danh mục, lọc theo danh mục, sắp xếp (mới nhất, giá tăng/giảm), phân trang.
- Tìm kiếm sản phẩm theo tên.
- Chi tiết sản phẩm: ảnh, giá, khuyến mãi, tồn kho, mô tả, sản phẩm cùng danh mục.
- Giỏ hàng lưu trong Session: thêm, đổi số lượng, xoá, tổng tiền tự tính.
- Đặt hàng (COD): form giao hàng, kiểm tra tồn kho trước khi tạo đơn.
- Lịch sử đơn hàng + theo dõi trạng thái từng bước.
- Đăng ký / đăng nhập bằng ASP.NET Core Identity (giao diện tiếng Việt, form giữa màn hình).

### Khu vực quản trị (vai trò Admin)
- Dashboard: doanh thu, số đơn, số sản phẩm, tài khoản, đơn gần đây,
  top sản phẩm theo doanh thu, cảnh báo sắp hết hàng.
- CRUD sản phẩm (ảnh, giá khuyến mãi, tồn kho, nổi bật) + tìm kiếm + phân trang.
- CRUD danh mục.
- Quản lý đơn hàng: lọc theo trạng thái, xem chi tiết, cập nhật trạng thái
  (chờ xác nhận → đang xử lý → đang giao → hoàn thành / đã hủy).
  Khi hủy đơn hệ thống tự hoàn tồn kho.

## 2. Công nghệ

| Thành phần | Công nghệ |
|---|---|
| Framework | ASP.NET Core 8 MVC (Razor Views) |
| ORM | Entity Framework Core 8 (SQL Server) |
| Auth | ASP.NET Core Identity (cookie), phân quyền Admin / Khách hàng |
| Session | ASP.NET Core Session cho giỏ hàng |
| Frontend | Bootstrap 5 + CSS tùy chỉnh phong cách Apple (đã đóng gói trong `wwwroot/`) |
| Database | SQL Server 2022 (Docker) |

## 3. Cấu trúc dự án

```
AppleStore/
├── Program.cs               # Cấu hình DI, Identity, Session, tự migrate + seed khi khởi động
├── Controllers/             # Home, Products, Cart, Orders
├── Areas/
│   ├── Admin/               # Khu vực quản trị (Dashboard, Products, Categories, Orders)
│   └── Identity/            # Trang Đăng nhập / Đăng ký (ghi đè giao diện Identity mặc định)
├── Components/              # View component (thanh danh mục)
├── Models/                  # Product, Category, Order, OrderItem, ApplicationUser...
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs          # Seed role Admin, tài khoản admin, 6 danh mục, 24 sản phẩm
├── Migrations/              # EF Core migrations (tạo schema database)
├── Services/
│   ├── SessionCart.cs       # Giỏ hàng trong Session
│   └── VietnameseIdentityErrorDescriber.cs  # Thông báo lỗi Identity tiếng Việt
├── ViewModels/
├── Views/
├── wwwroot/
│   ├── images/products/     # Ảnh sản phẩm thật
│   └── lib/                 # Bootstrap, jQuery, jQuery Validation (không cần npm)
└── tools/fetch_images.py    # Script tải ảnh sản phẩm (tuỳ chọn, cần Python)
```

---

## 4. Cài đặt & chạy trên máy mới

Mục tiêu: từ một máy trống đến website chạy tại `http://localhost:5264`.

### 4.1 Yêu cầu hệ thống

| Thành phần | Phiên bản | Bắt buộc |
|---|---|---|
| .NET SDK | 8.0.x (dự án pin `8.0.410` trong `global.json`) | ✔ |
| Docker Desktop / Docker Engine | bản bất kỳ còn hoạt động | ✔ *(hoặc đã cài SQL Server)* |
| SQL Server | 2022 (image Docker) hoặc bản đã cài sẵn | ✔ |
| Python 3 | — | chỉ cần khi muốn chạy lại `tools/fetch_images.py` |

> Frontend (Bootstrap, jQuery, ảnh sản phẩm) đã nằm sẵn trong `wwwroot/` →
> **không cần cài Node.js / npm**.

### 4.2 Bước 1 — Lấy source code

```bash
git clone <đường-dẫn-repo> AppleStore
cd AppleStore
```

Không dùng Git thì copy nguyên cả thư mục dự án sang máy mới.
Nếu copy kèm thư mục `bin/`, `obj/` (file build của máy cũ) thì hãy xoá chúng —
`dotnet restore` sẽ tạo lại:

```bash
rm -rf bin obj          # macOS / Linux
del /s /q bin obj       # Windows
```

### 4.3 Bước 2 — Cài .NET SDK 8

```bash
dotnet --version        # phải in ra một phiên bản 8.0.x
```

Chưa có: tải tại <https://dotnet.microsoft.com/download/dotnet/8.0>.

Dự án có `global.json` (`version: 8.0.410`, `rollForward: latestFeature`) nên chỉ cần
một bản .NET SDK **8.0.x bất kỳ** là đủ. Nếu máy chỉ có .NET 9/10 thì sẽ báo
`A compatible installed .NET SDK for global.json was not found` → cài thêm .NET 8 SDK.

### 4.4 Bước 3 — Chạy SQL Server bằng Docker

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=StrongP@ss123" \
  -p 1433:1433 -d --name sqlserver2022 mcr.microsoft.com/mssql/server:2022-latest
```

- Mật khẩu `StrongP@ss123` **khớp sẵn** với `appsettings.json` → không cần cấu hình gì thêm.
- Lần đầu Docker tải image ~1,5 GB; SQL Server cần **10–30 giây** để khởi động.
- Kiểm tra trạng thái: `docker ps` → cột `STATUS` phải là `Up`.
- macOS/Windows: mở Docker Desktop trước khi chạy lệnh.
- Đã có SQL Server sẵn trên máy? Bỏ qua bước này và sửa chuỗi kết nối ở mục 4.5.

### 4.5 Bước 4 — Cấu hình chuỗi kết nối *(nếu khác mặc định)*

File `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=AppleStore;User Id=sa;Password=StrongP@ss123;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

| Thành phần | Ý nghĩa |
|---|---|
| `Server=localhost,1433` | host + port của SQL Server |
| `Database=AppleStore` | tên database — được tạo tự động khi chạy lần đầu |
| `User Id=sa`, `Password=...` | phải trùng với `MSSQL_SA_PASSWORD` lúc chạy Docker |
| `TrustServerCertificate=True` | bỏ qua chứng chỉ self-signed (chỉ dùng khi dev) |

### 4.6 Bước 5 — Khôi phục gói & chạy

```bash
dotnet tool restore      # khôi phục công cụ dotnet-ef (1 lần / máy)
dotnet restore           # khôi phục gói NuGet (1 lần / máy)
dotnet run --launch-profile http
```

- Website chạy tại **http://localhost:5264** — dừng bằng `Ctrl + C`.
- **Không cần chạy tay `dotnet ef database update`:** khi khởi động, `Program.cs` tự
  gọi `db.Database.MigrateAsync()` → tạo database `AppleStore` + toàn bộ bảng từ
  thư mục `Migrations/`, rồi `DbSeeder.SeedAsync()` → tạo role Admin, tài khoản admin,
  6 danh mục và 24 sản phẩm mẫu.
- Những lần chạy sau chỉ cần: `dotnet run --launch-profile http`.

### 4.7 Bước 6 — Kiểm tra hoạt động

Mở trình duyệt truy cập `http://localhost:5264`. Trang chủ hiện banner + danh mục +
sản phẩm (có ảnh) là đã chạy thành công.

| Trang | URL |
|---|---|
| Trang chủ | `http://localhost:5264/` |
| Danh sách sản phẩm | `http://localhost:5264/Products` |
| Giỏ hàng | `http://localhost:5264/Cart` |
| Đăng nhập | `http://localhost:5264/Identity/Account/Login` |
| Đăng ký | `http://localhost:5264/Identity/Account/Register` |
| Quản trị *(cần vai trò Admin)* | `http://localhost:5264/Admin/Dashboard` |

### 4.8 HTTPS *(tuỳ chọn)*

```bash
dotnet dev-certs https --trust      # tin cậy chứng chỉ dev
dotnet run --launch-profile https   # https://localhost:7120 (+ http://localhost:5264)
```

### 4.9 Khi thay đổi Model/Entity

```bash
dotnet ef migrations add <TenMigration>
dotnet ef database update        # hoặc chỉ chạy lại app — Program.cs tự update
```

Lưu ý: lệnh là **`dotnet ef`** (sau khi đã `dotnet tool restore`), không phải
`dotnet dotnet-ef`.

### 4.10 Bảng cổng theo profile

| Profile | URL |
|---|---|
| `http` | `http://localhost:5264` |
| `https` | `https://localhost:7120` và `http://localhost:5264` |
| `IIS Express` | `http://localhost:23161` *(chỉ Windows, chạy từ Visual Studio)* |

---

## 5. Tài khoản mặc định

Chỉ **một tài khoản** được seed (xem `Data/DbSeeder.cs`):

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Admin | `admin@applestore.vn` | `Admin@123` |

- Khách hàng tự đăng ký tại `/Identity/Account/Register`; tài khoản thường không
  thuộc vai trò Admin nên không vào được `/Admin`.
- Chính sách mật khẩu (khởi tạo ở `Program.cs`): tối thiểu 6 ký tự, phải có chữ hoa
  + chữ thường + chữ số, **không** bắt buộc ký tự đặc biệt.
- Mật khẩu admin bị hardcode trong source — chỉ dành cho môi trường học tập,
  **không dùng cho production**.

## 6. Xử lý sự cố

| Tình trạng | Nguyên nhân & cách xử lý |
|---|---|
| `A network-related or instance-specific error occurred` / `Cannot open server "localhost"` | Docker hoặc SQL Server chưa chạy (hoặc chưa khởi động xong) → `docker ps`, chờ 10–30 giây, xem `docker logs sqlserver2022` |
| `Login failed for user 'sa'` | Mật khẩu trong `appsettings.json` khác `MSSQL_SA_PASSWORD` lúc chạy Docker → khớp lại hai nơi |
| `A compatible installed .NET SDK for global.json was not found` | Máy chưa cài .NET 8 SDK → cài bản 8.0.x (mục 4.3) |
| `Address already in use` (cổng 5264) | Đã có tiến trình khác chiếm cổng → tắt tiến trình cũ, hoặc chạy `dotnet run --launch-profile http --urls "http://localhost:5000"` |
| `dotnet: command not found` | Cài SDK xong chưa mở lại terminal / PATH chưa nạp → mở terminal mới |
| Ảnh sản phẩm không hiện (404) | Copy thiếu `wwwroot/images` → copy lại toàn bộ thư mục `wwwroot` |
| Trang trắng / lỗi 500 | Xem log trực tiếp trên console của `dotnet run` (môi trường Development hiển thị chi tiết lỗi) |
| Muốn reset database + seed lại từ đầu | Xoá database `AppleStore` rồi chạy lại app, ví dụ:<br>`docker exec -it sqlserver2022 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'StrongP@ss123' -Q "DROP DATABASE AppleStore;"`<br>*(image cũ dùng đường dẫn `/opt/mssql-tools/bin/sqlcmd`)* |

## 7. Kiểm thử

- Build: `dotnet build` → 0 lỗi, 0 cảnh báo.
- Luồng khách hàng (đã kiểm thử trên trình duyệt): đăng ký → đăng nhập →
  thêm giỏ hàng → đặt hàng → xem lịch sử đơn.
- Form Đăng nhập/Đăng ký: validation tiếng Việt (thiếu chữ hoa, thiếu chữ số,
  email sai…), open-redirect được chặn.
- Luồng quản trị: đăng nhập Admin → dashboard → CRUD sản phẩm/danh mục →
  cập nhật trạng thái đơn hàng (hủy đơn tự hoàn tồn kho).

## 8. Ghi chú học tập

- Ảnh sản phẩm lấy từ Wikimedia Commons (script `tools/fetch_images.py`).
- Nhãn hiệu Apple chỉ phục vụ mục đích học tập.
