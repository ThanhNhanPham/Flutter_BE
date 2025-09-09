# FoodOrdering_BE — Backend API

**FoodOrdering_BE** là dịch vụ **ASP.NET Core Web API** cho hệ thống đặt đồ ăn. Repo tổ chức theo kiến trúc controller–service–repository, hỗ trợ quản lý sản phẩm, giỏ hàng, đơn hàng, người dùng… và có sẵn QR code utilities.

## 🔧 Công nghệ & yêu cầu môi trường

- .NET 7+ (khuyến nghị .NET 8)
- ASP.NET Core Web API
- Entity Framework Core (Migrations)
- SQL Server (tùy `appsettings.json`)
- Visual Studio 2022 / Rider / VS Code + C# extensions
- Swagger

---

## 🗂️ Cấu trúc thư mục chính
FoodOrdering_BE
├── Controllers
│   ├── AuthenticationController.cs
│   ├── CartController.cs
│   ├── CategoryController.cs
│   ├── FavoriteController.cs
│   ├── OrderController.cs
│   ├── ProductController.cs
│   ├── QRCodeController.cs
│   └── UserController.cs
├── DTO/
├── Hubs/               
├── Migrations/
├── Model/
├── Repository/
├── wwwroot/           
├── appsettings.json
├── Program.cs
└── FoodOrdering.http  
```

**Thực thể (gợi ý):** `User`, `Product`, `Category`, `Cart`, `CartItem`, `Order`, `OrderItem`, `Favorite`, …

---

## ▶️ Chạy dự án (local)

1. **Cấu hình biến môi trường / connection string**
   - Mở `appsettings.json` và sửa kết nối DB (ví dụ SQL Server):
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "Server=localhost;Database=FoodOrdering;Trusted_Connection=True;TrustServerCertificate=True"
       },
       "Jwt": {
         "Issuer": "FoodOrdering",
         "Audience": "FoodOrdering",
         "Key": "your-very-long-secret-key"
       }
     }
     ```

2. **Áp dụng migration & tạo DB**
   ```bash
   dotnet tool restore
   dotnet ef database update
   ```
   > Nếu chưa có migration đầu tiên: `dotnet ef migrations add InitialCreate`

3. **Chạy API**
   ```bash
   dotnet build
   dotnet run --project FoodOrdering_BE
   ```

4. **Tài liệu API**
   - Mặc định: `https://localhost:5001/swagger` hoặc `http://localhost:5000/swagger` (tuỳ cấu hình).

---

## 🔐 Xác thực & phân quyền (gợi ý)

- JWT Bearer Token cho các endpoint cần bảo vệ.
- Flow mẫu: **Đăng ký → Đăng nhập → nhận AccessToken/RefreshToken → gọi API kèm `Authorization: Bearer <token>`**.
- Phân quyền theo vai trò: `Admin`, `User` (nếu có).

## 📚 Danh mục API (tóm tắt theo Controller)

> Lưu ý: Tên route có thể thay đổi. Kiểm tra `[Route]` / `[HttpGet/Post/Put/Delete]` trong các Controller hoặc Swagger để biết chính xác.

### 1) `AuthenticationController`
- `POST /api/auth/register` – Đăng ký tài khoản
- `POST /api/auth/login` – Đăng nhập, nhận JWT
- `POST /api/auth/refresh-token` – Làm mới AccessToken
- `POST /api/auth/logout` – Đăng xuất / revoke (nếu có)

### 2) `UserController`
- `GET /api/users/me` – Lấy thông tin cá nhân
- `PUT /api/users/me` – Cập nhật hồ sơ
- `PUT /api/users/change-password` – Đổi mật khẩu
- `GET /api/users` – (Admin) Danh sách người dùng
- `GET /api/users/{id}` / `DELETE /api/users/{id}` – (Admin)

### 3) `CategoryController`
- `GET /api/categories` – Danh sách danh mục
- `GET /api/categories/{id}` – Chi tiết
- `POST /api/categories` – Tạo (Admin)
- `PUT /api/categories/{id}` – Sửa (Admin)
- `DELETE /api/categories/{id}` – Xoá (Admin)

### 4) `ProductController`
- `GET /api/products` – Danh sách, hỗ trợ **filter/search/pagination**
- `GET /api/products/{id}` – Chi tiết
- `POST /api/products` – Tạo (Admin)
- `PUT /api/products/{id}` – Sửa (Admin)
- `DELETE /api/products/{id}` – Xoá (Admin)

### 5) `CartController`
- `GET /api/carts` – Lấy giỏ của user hiện tại
- `POST /api/carts/items` – Thêm sản phẩm vào giỏ
- `PUT /api/carts/items/{itemId}` – Cập nhật số lượng
- `DELETE /api/carts/items/{itemId}` – Xoá item
- `DELETE /api/carts/clear` – Xoá toàn bộ giỏ

### 6) `FavoriteController`
- `GET /api/favorites` – Danh sách yêu thích
- `POST /api/favorites/{productId}` – Thêm
- `DELETE /api/favorites/{productId}` – Gỡ

### 7) `OrderController`
- `GET /api/orders` – Danh sách đơn của user (hoặc Admin xem tất cả)
- `GET /api/orders/{id}` – Chi tiết
- `POST /api/orders/checkout` – Tạo đơn từ giỏ hàng
- `PUT /api/orders/{id}/status` – Cập nhật trạng thái (Admin)
- (Tuỳ chọn) webhook thanh toán: `/api/orders/payment/webhook`

### 8) `QRCodeController`
- `GET /api/qrcode?data=...` – Sinh ảnh QR cho chuỗi dữ liệu
- (Tuỳ chọn) `POST /api/qrcode` – Sinh QR từ payload

---

## 🧱 Repository & DTO

- **Repository/**: Chứa lớp truy cập dữ liệu (interface + implementation), giúp tách biệt logic domain khỏi EF Core.
- **DTO/**: Định nghĩa các `InputDto`/`OutputDto` dùng cho request/response, tránh lộ model nội bộ.

> Gợi ý: xác thực dữ liệu với `DataAnnotations` hoặc `FluentValidation`.

---

## 🗃️ Migrations — lệnh thường dùng

```bash
dotnet ef migrations add <Name>            # tạo migration
dotnet ef database update                  # áp dụng migration
dotnet ef database update 0                # rollback toàn bộ
dotnet ef migrations remove                # xoá migration mới nhất (chưa apply)


## 🧪 Kiểm thử nhanh bằng file `.http` (Visual Studio/VS Code)

- Mở **FoodOrdering.http** → gửi request trực tiếp tới API (tiện debug).
- Hoặc dùng Postman/Insomnia. Nên tạo **collection** kèm sẵn biến `{{baseUrl}}`, token…

---

## 🚀 Triển khai (gợi ý)

- **IIS / Windows Service** hoặc **Docker**:
  ```dockerfile
  # ví dụ tối thiểu
  FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
  WORKDIR /app
  EXPOSE 8080

  FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
  WORKDIR /src
  COPY . .
  RUN dotnet restore
  RUN dotnet publish -c Release -o /out

  FROM base AS final
  WORKDIR /app
  COPY --from=build /out .
  ENTRYPOINT ["dotnet", "FoodOrdering_BE.dll"]
  ```
- Cấu hình biến môi trường qua `appsettings.Production.json` + `DOTNET_ENVIRONMENT=Production`.

---

## 🔒 Bảo mật & best practices

- Không commit **secret** (JWT key, connection string) vào repo công khai.
- Bật **HTTPS**, **CORS** đúng domain FE.
- Sử dụng **Response caching**, **Pagination**, **Rate limiting** (nếu public).
- Log tập trung (Serilog) & theo dõi lỗi (Sentry/Application Insights).

---

## 🧭 Lộ trình phát triển (suggestions)

- Phân quyền theo vai trò (Admin/User).
- Tìm kiếm nâng cao sản phẩm (price range, category, keyword).
- Payment gateway (VNPAY/MoMo/Stripe) + webhook.
- Realtime order status (SignalR trong `Hubs/`).
- Seed dữ liệu mẫu (Categories, Products).
