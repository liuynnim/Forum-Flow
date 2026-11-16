# 🔥 Forum-Flow — Modern Community Forum & Real-time Platform

**Forum-Flow** là nền tảng diễn đàn thảo luận cộng đồng hiện đại, được xây dựng theo kiến trúc **Clean Architecture + CQRS** ở phía Backend (**ASP.NET Core 10**) và **Next.js App Router** ở phía Frontend.

---

## 🏗️ Kiến trúc & Công nghệ Sử dụng (Tech Stack)

### 1. Backend (`forum-flow-be`)
* **Framework:** .NET 10 / ASP.NET Core Web API
* **Kiến trúc:** Clean Architecture (4 tầng tách biệt: `Domain`, `Application`, `Infrastructure`, `Api`) kết hợp mẫu **CQRS (MediatR)**
* **Cơ sở dữ liệu & ORM:** PostgreSQL + Entity Framework Core 10 (Code-First Migrations, Check Constraints, Indexing)
* **Bảo mật & Xác thực:** JWT Bearer Authentication, BCrypt Password Hashing, Role-Based Access Control (`GUEST`, `MEMBER`, `MODERATOR`, `ADMIN`)
* **Real-time Communication:** ASP.NET Core SignalR (`ChatHub`, `NotificationHub`)
* **Validation & Chuẩn hóa API:** FluentValidation, chuẩn hóa Response Envelope (`ApiResponse<T>`)

### 2. Frontend (`forum-flow-fe`)
* **Framework:** Next.js 16 (App Router) + React 19 + TypeScript
* **UI & Styling:** Tailwind CSS v4 + Shadcn UI + Lucide Icons
* **State Management:** Zustand
* **HTTP & Real-time Client:** Axios (JWT Interceptors) + `@microsoft/signalr`

---

## 📂 Cấu trúc Thư mục Dự án

```text
Forum-Flow/
├── Detailed Specification/                  # Bộ tài liệu đặc tả kỹ thuật chi tiết
│   ├── Chapter-01-Requirements-Specification.md
│   ├── Chapter-02-System-Architecture.md
│   ├── Chapter-03-Database-Design.md
│   └── Chapter-04-API-Specification.md
├── forum-flow-be/                           # Backend Solution (.NET 10 Clean Architecture)
│   ├── ForumFlow.Domain/                    # Entities, Enums, Core Domain Rules
│   ├── ForumFlow.Application/               # CQRS Commands/Queries, DTOs, Interfaces, ApiResponse
│   ├── ForumFlow.Infrastructure/            # EF Core DbContext, Migrations, JWT Service, DI
│   └── ForumFlow.Api/                       # REST Controllers, SignalR Hubs, Middleware Pipeline
└── forum-flow-fe/                           # Frontend Application (Next.js App Router)
    └── src/
        ├── app/                             # App Router Pages & Layouts
        ├── components/                      # Reusable UI & Layout Components
        └── lib/                             # Axios, SignalR & Utility Helpers
```

---

## ✨ Tiến độ Tính năng (Feature Roadmap)

- [x] **Đặc tả Hệ thống & Thiết kế CSDL:** Hoàn thiện 4 chương đặc tả (Requirements, Architecture, DB Schema, REST/SignalR API Spec).
- [x] **Khởi tạo Clean Architecture & EF Core Migrations:** Thiết lập đầy đủ các bảng cốt lõi (`Users`, `Categories`, `Posts`, `Tags`, `PostTags`, `PostVotes`, `Bookmarks`, `Comments`, `CommentVotes`, `PostEditHistories`).
- [x] **Xác thực & Phân quyền (Auth):** Đăng ký (`POST /api/auth/register`) và Đăng nhập (`POST /api/auth/login`) với CQRS MediatR + JWT Token.
- [x] **Quản lý Chuyên mục (`Categories`):** Lấy danh sách chuyên mục kèm số lượng bài viết (`GET /api/categories`) và tạo chuyên mục mới (`POST /api/categories`).
- [x] **Quản lý Bài viết & Tương tác (`Posts`):**
  - Danh sách bài viết phân trang, lọc theo chuyên mục, sắp xếp `newest` / `hot` (`GET /api/posts`).
  - Xem chi tiết bài viết theo Slug kèm Tags & Tác giả (`GET /api/posts/{slug}`).
  - Đăng bài viết mới tự động sinh Slug SEO & gắn Tags (`POST /api/posts`).
  - Chỉnh sửa bài viết kèm lưu lịch sử `PostEditHistory` (`PUT /api/posts/{id}`).
  - Xóa mềm bài viết (`DELETE /api/posts/{id}`).
  - Upvote / Downvote (`POST /api/posts/{id}/vote`) & Bookmark bài viết (`POST /api/posts/{id}/bookmark`).
- [x] **Chuẩn hóa Response Envelope & Bảo mật Cấu hình:** Tự động chuẩn hóa lỗi Validation theo Spec và tách cấu hình nhạy cảm sang `appsettings.Local.json`.
- [ ] **Hệ thống Bình luận phân cấp (`Comments`):** Đang triển khai.
- [ ] **Giao diện Frontend (`forum-flow-fe`):** Đang đồng bộ với các API của Backend.
- [ ] **Real-time Chat:** *(Tạm hoãn để ưu tiên hoàn thiện trọn vẹn các tính năng Forum Core)*.

---

## 🚀 Hướng dẫn Cài đặt & Chạy Dự án (Getting Started)

### Yêu cầu môi trường
* **.NET SDK:** 10.0 trở lên
* **Database:** PostgreSQL (chạy ở cổng mặc định `5432`)
* **Node.js:** 20+ và **pnpm**

---

### 1. Thiết lập và chạy Backend (`forum-flow-be`)

**Bước 1:** Di chuyển vào thư mục Backend:
```bash
cd forum-flow-be
```

**Bước 2:** Tạo file cấu hình cá nhân `ForumFlow.Api/appsettings.Local.json` (file này đã được `.gitignore` bảo vệ, không bị đẩy lên Git):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=forumflow_db;Username=postgres;Password=YOUR_POSTGRES_PASSWORD"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SUPER_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG",
    "Issuer": "ForumFlow",
    "Audience": "ForumFlowClient",
    "ExpiresInHours": 24
  }
}
```

**Bước 3:** Cập nhật Database (EF Core Migration):
```bash
dotnet ef database update --project ForumFlow.Infrastructure --startup-project ForumFlow.Api
```

**Bước 4:** Khởi chạy Backend API:
```bash
dotnet run --project ForumFlow.Api
```
* Server sẽ chạy tại: `http://localhost:5041`
* OpenAPI JSON: `http://localhost:5041/openapi/v1.json`

---

### 2. Thiết lập và chạy Frontend (`forum-flow-fe`)

**Bước 1:** Di chuyển vào thư mục Frontend và cài đặt dependencies:
```bash
cd forum-flow-fe
pnpm install
```

**Bước 2:** Tạo file `.env.local` tại `forum-flow-fe/`:
```env
NEXT_PUBLIC_API_URL=http://localhost:5041/api
NEXT_PUBLIC_SIGNALR_URL=http://localhost:5041/hubs
```

**Bước 3:** Chạy server phát triển:
```bash
pnpm dev
```
* Truy cập ứng dụng tại: `http://localhost:3000`

---

## 📖 Tài liệu Đặc tả Chi tiết

* [Chương I: Xác định Yêu cầu Hệ thống (Requirements Specification)](./Detailed%20Specification/Chapter-01-Requirements-Specification.md)
* [Chương II: Kiến trúc Hệ thống (System Architecture)](./Detailed%20Specification/Chapter-02-System-Architecture.md)
* [Chương III: Thiết kế Cơ sở Dữ liệu (Database Design)](./Detailed%20Specification/Chapter-03-Database-Design.md)
* [Chương IV: Đặc tả API (REST & SignalR API Specification)](./Detailed%20Specification/Chapter-04-API-Specification.md)
