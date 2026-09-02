# CHƯƠNG II: KIẾN TRÚC HỆ THỐNG
## (SYSTEM ARCHITECTURE)

> **Dự án:** Forum-Flow — Forum / Community Platform kết hợp Chat trực tiếp  
> **Phiên bản tài liệu:** 1.1.0  
> **Ngày tạo:** 2026-08-02  
> **Cập nhật:** 2026-08-02 — Đổi Frontend từ React+Vite SPA sang Next.js 15 (App Router) để tối ưu SEO  
> **Trạng thái:** Draft

---

## Mục lục

- [2.1. Tổng quan Kiến trúc](#21-tổng-quan-kiến-trúc)
- [2.2. Technology Stack](#22-technology-stack)
- [2.3. Chiến lược Render (Next.js Hybrid Rendering)](#23-chiến-lược-render-nextjs-hybrid-rendering)
- [2.4. Sơ đồ Kiến trúc Thành phần](#24-sơ-đồ-kiến-trúc-thành-phần-component-diagram)
- [2.5. Mô tả Chi tiết Từng Lớp](#25-mô-tả-chi-tiết-từng-lớp-layer-description)
- [2.6. Chiến lược Real-time với SignalR](#26-chiến-lược-real-time-với-signalr)
- [2.7. Luồng Xác thực (Auth Flow)](#27-luồng-xác-thực-auth-flow)
- [2.8. Chiến lược Lưu trữ File](#28-chiến-lược-lưu-trữ-file-cloud-storage)
- [2.9. Chiến lược Caching & Rate Limiting](#29-chiến-lược-caching--rate-limiting)
- [2.10. Kiến trúc Triển khai (Deployment Architecture)](#210-kiến-trúc-triển-khai-deployment-architecture)
- [2.11. Cấu trúc Thư mục Dự án](#211-cấu-trúc-thư-mục-dự-án-project-structure)

---

## 2.1. Tổng quan Kiến trúc

### Mô hình kiến trúc: **Monolithic Modular Architecture**

Forum-Flow áp dụng kiến trúc **Monolithic có cấu trúc module hóa** — tất cả business logic nằm trong một ứng dụng ASP.NET Core 8 duy nhất, nhưng được tổ chức theo từng module độc lập (Auth, Forum, Chat, Notification, Admin). Đây là lựa chọn phù hợp cho giai đoạn đầu:

| Tiêu chí | Quyết định | Lý do |
|---------|-----------|-------|
| **Backend pattern** | Monolithic Modular | Đơn giản hóa deploy, dễ maintain, phù hợp quy mô ban đầu |
| **API style** | RESTful API + SignalR | REST cho CRUD, SignalR cho real-time (Chat, Notifications) |
| **Render strategy** | **Hybrid Rendering (Next.js 15)** | SSR/SSG/ISR cho trang public (SEO), CSR cho Chat/Admin |
| **SEO approach** | Server-Side Rendering + Open Graph | Bài viết được Google index ngay, social preview hoạt động |
| **Database strategy** | Single Database + Read Replica (tương lai) | PostgreSQL làm DB chính, Redis làm cache |
| **Scalability path** | Horizontal scaling (Load Balancer) | Sticky sessions với Redis backplane cho SignalR |

---

## 2.2. Technology Stack

### 🎨 Frontend Layer

| Công nghệ | Phiên bản | Vai trò |
|-----------|-----------|---------|
| **Next.js** | 15.x (App Router) | React framework — Hybrid rendering (SSR/SSG/ISR/CSR) |
| **React.js** | 19.x (bundled) | UI library — tích hợp sẵn trong Next.js |
| **TypeScript** | 5.x | Type safety toàn bộ codebase |
| **TanStack Query (React Query)** | 5.x | Server state: cache, pagination, background refetch |
| **Zustand** | 4.x | Client-side state (auth session, UI state) |
| **@microsoft/signalr** | 8.x | SignalR client SDK cho real-time (Chat, Notifications) |
| **TipTap** | Latest | Rich Text / Markdown editor cho bài viết |
| **Axios** | 1.x | HTTP client, interceptors cho JWT refresh |
| **React Hook Form + Zod** | Latest | Form management + schema validation |
| **Shadcn/UI + Tailwind CSS** | Latest | Component library + utility CSS |
| **next-themes** | Latest | Dark/Light mode management |
| **next-seo** | Latest | SEO meta tags, Open Graph, JSON-LD |

### ⚙️ Backend Layer

| Công nghệ | Phiên bản | Vai trò |
|-----------|-----------|---------|
| **ASP.NET Core** | 8.x (LTS) | Web framework chính (REST API + SignalR) |
| **SignalR** | Built-in .NET 8 | Real-time: Chat, Notifications, Typing, Online status |
| **ASP.NET Core Identity** | Built-in | User management, password hashing, roles |
| **Entity Framework Core** | 8.x | ORM — truy vấn & migration database |
| **Npgsql EF Provider** | 8.x | PostgreSQL driver cho EF Core |
| **StackExchange.Redis** | 2.x | Redis client cho caching & SignalR backplane |
| **MediatR** | 12.x | Mediator pattern — CQRS (Commands & Queries) |
| **FluentValidation** | 11.x | Validation pipeline cho request models |
| **Serilog** | Latest | Structured logging (file, console, Seq) |
| **Swagger / Swashbuckle** | Latest | API documentation (OpenAPI 3.0) |
| **Hangfire** | Latest | Background jobs (email queue, cleanup tasks) |
| **Cloudinary SDK** | Latest | Upload & quản lý file/ảnh |

### 🔐 Authentication & Authorization

| Công nghệ | Vai trò |
|-----------|---------|
| **JWT (JSON Web Token)** | Stateless authentication — Access Token (15 phút) + Refresh Token (7 ngày) |
| **OAuth2 / OpenID Connect** | Đăng nhập qua Google, GitHub |
| **ASP.NET Core Identity** | Quản lý user, role, claim |
| **Policy-based Authorization** | Mapping RBAC (GUEST → MEMBER → MODERATOR → ADMIN) |

### 🗄️ Data Layer

| Công nghệ | Phiên bản | Vai trò |
|-----------|-----------|---------|
| **PostgreSQL** | 16.x | Database quan hệ chính — lưu trữ toàn bộ dữ liệu |
| **Redis** | 7.x | Cache L2, SignalR Backplane, Rate Limiting, Refresh Tokens |
| **Cloudinary** | CDN | Lưu trữ & phân phối ảnh/file (avatar, ảnh bài viết, file chat) |

### 🛠️ DevOps & Infrastructure

| Công nghệ | Vai trò |
|-----------|---------|
| **Docker + Docker Compose** | Container hóa tất cả services |
| **Nginx** | Reverse proxy, SSL termination, static file serving |
| **GitHub Actions** | CI/CD pipeline |
| **Seq / Grafana** | Log management & monitoring |

---

## 2.3. Chiến lược Render (Next.js Hybrid Rendering)

Next.js 15 cho phép mỗi route chọn chiến lược render **độc lập** — đây là lợi thế lớn nhất so với SPA thuần túy:

| Route | Chiến lược | Lý do |
|-------|-----------|-------|
| `/` — Trang chủ | **SSG** | Danh sách categories ít thay đổi → build 1 lần, serve cực nhanh |
| `/c/[slug]` — Danh sách bài theo Category | **ISR** (5 phút) | Cần fresh data nhưng không phải real-time → tự động rebuild |
| `/p/[slug]` — Chi tiết bài viết | **SSR** | Nội dung luôn mới nhất, meta tags Open Graph đúng |
| `/u/[username]` — Profile người dùng | **SSR** | Dữ liệu cần cập nhật, SEO quan trọng |
| `/search` — Tìm kiếm | **SSR** | Query params thay đổi mỗi request |
| `/login`, `/register` | **CSR** | Không cần SEO, auth pages |
| `/chat` | **CSR** | Real-time, không cần SEO |
| `/notifications` | **CSR** | Dữ liệu riêng tư, không cần SEO |
| `/settings` | **CSR** | Trang cá nhân |
| `/admin/*` | **CSR** | Dashboard nội bộ |
| `/moderator/*` | **CSR** | Công cụ nội bộ |

### SEO với SSR — Ví dụ Chi tiết Bài viết

```
Googlebot / User truy cập: /p/huong-dan-signalr-dotnet8
            │
            ▼
     Next.js Server (Node.js)
            │── fetch dữ liệu từ .NET API
            │── render HTML đầy đủ:
            │
            │   <head>
            │     <title>Hướng dẫn SignalR - Forum-Flow</title>
            │     <meta name="description" content="SignalR là..." />
            │     <meta property="og:title" content="..." />
            │     <meta property="og:image" content="..." />
            │   </head>
            │   <body>
            │     <h1>Hướng dẫn dùng SignalR trong .NET 8</h1>
            │     <article>Nội dung đầy đủ...</article>
            │   </body>
            │
            ▼
    Google index ngay ✅ | Social share preview ✅ | LCP nhanh ✅
```

---

## 2.4. Sơ đồ Kiến trúc Thành phần (Component Diagram)

```
╔═══════════════════════════════════════════════════════════════════╗
║                     FRONTEND LAYER (Next.js 15)                   ║
║                                                                   ║
║  ┌─────────────────────────────────────────────────────────────┐  ║
║  │                    Next.js App Router                        │  ║
║  │                                                              │  ║
║  │  ┌────────────────────┐    ┌──────────────────────────────┐  │  ║
║  │  │  SERVER COMPONENTS  │    │    CLIENT COMPONENTS (CSR)   │  │  ║
║  │  │  (SSR / SSG / ISR) │    │                              │  │  ║
║  │  │                    │    │  ┌──────────┐ ┌───────────┐  │  │  ║
║  │  │  /              SSG│    │  │  Chat    │ │  Admin    │  │  │  ║
║  │  │  /c/[slug]      ISR│    │  │  Module  │ │ Dashboard │  │  │  ║
║  │  │  /p/[slug]      SSR│    │  │(SignalR) │ │           │  │  │  ║
║  │  │  /u/[username]  SSR│    │  └────┬─────┘ └───────────┘  │  │  ║
║  │  │  /search        SSR│    │       │ (SignalR WSS)         │  │  ║
║  │  └────────┬───────────┘    └───────┼──────────────────────┘  │  ║
║  │           │ (fetch REST API)       │                          │  ║
║  └───────────┼───────────────────────┼──────────────────────────┘  ║
╚═════════════╪═══════════════════════╪══════════════════════════════╝
              │ HTTPS:443             │ WSS (WebSocket)
              ▼                       ▼
╔═══════════════════════════════════════════════════════════════════╗
║                       NGINX REVERSE PROXY                         ║
║   - SSL Termination (Let's Encrypt)                               ║
║   - Route: /api/* → ASP.NET Core :5000                           ║
║   - Route: /hubs/* → ASP.NET Core :5000 (WebSocket upgrade)      ║
║   - Route: /* → Next.js Server :3000                             ║
╚═══════════════════════╦═══════════════════════════════════════════╝
                        ║
╔═══════════════════════▼═══════════════════════════════════════════╗
║                 ASP.NET CORE 8 APPLICATION                        ║
║                                                                   ║
║  ┌───────────────────────────────────────────────────────────┐    ║
║  │                    API GATEWAY LAYER                       │    ║
║  │   Authentication Middleware │ Rate Limiting │ CORS         │    ║
║  └──────────────────────┬────────────────────────────────────┘    ║
║                         │                                         ║
║  ┌──────────────────────▼────────────────────────────────────┐    ║
║  │              CONTROLLER LAYER (REST API)                   │    ║
║  │  AuthController │ PostsController │ UsersController        │    ║
║  │  CommentsController │ ChatController │ AdminController     │    ║
║  └──────────────────────┬────────────────────────────────────┘    ║
║                         │ MediatR                                 ║
║  ┌──────────────────────▼────────────────────────────────────┐    ║
║  │              APPLICATION LAYER (CQRS)                      │    ║
║  │   Commands (Write) │ Queries (Read) │ Validators           │    ║
║  └──────────────────────┬────────────────────────────────────┘    ║
║                         │                                         ║
║  ┌──────────────────────▼────────────────────────────────────┐    ║
║  │                   DOMAIN LAYER                             │    ║
║  │    Entities │ Domain Services │ Repository Interfaces      │    ║
║  └──────────────────────┬────────────────────────────────────┘    ║
║                         │                                         ║
║  ┌──────────────────────▼────────────────────────────────────┐    ║
║  │               INFRASTRUCTURE LAYER                         │    ║
║  │   EF Core (PostgreSQL) │ Redis │ Cloudinary │ Email        │    ║
║  └───────────────────────────────────────────────────────────┘    ║
║                                                                   ║
║  ┌───────────────────────────────────────────────────────────┐    ║
║  │              SIGNALR HUBS (Real-time)                      │    ║
║  │    ChatHub │ NotificationHub │ PresenceHub (Online)        │    ║
║  └───────────────────────────────────────────────────────────┘    ║
╚═══════════════════════════════════════════════════════════════════╝
              │                         │
   ┌──────────▼──────────┐   ┌──────────▼──────────┐
   │    PostgreSQL 16    │   │       Redis 7         │
   │   (Primary DB)      │   │  Cache / Backplane   │
   └─────────────────────┘   └─────────────────────┘
```

---

## 2.5. Mô tả Chi tiết Từng Lớp (Layer Description)

### 2.5.1. Frontend Layer (Next.js 15)

Next.js 15 với App Router kết nối với backend qua 2 kênh:
- **fetch / Axios** — HTTP client cho REST API calls. Server Components dùng `fetch()` native của Node.js; Client Components dùng Axios với JWT interceptor.
- **@microsoft/signalr** — Kết nối WebSocket tới SignalR Hubs — **chỉ khởi tạo ở Client Components** (không thể dùng trong Server Components).

**Phân loại routes theo render strategy:**

| Route/Module | Render | Màn hình |
|-------------|--------|----------|
| `/` | SSG | Trang chủ — danh sách Categories |
| `/c/[slug]` | ISR | Danh sách bài viết theo Category |
| `/p/[slug]` | SSR | Chi tiết bài viết + Comments |
| `/u/[username]` | SSR | Profile công khai người dùng |
| `/search` | SSR | Kết quả tìm kiếm |
| `/login`, `/register` | CSR | Auth pages |
| `/chat` | CSR | Chat 1-1 và Group (SignalR) |
| `/notifications` | CSR | Danh sách thông báo |
| `/settings` | CSR | Cài đặt tài khoản |
| `/admin/*` | CSR | Admin Dashboard |
| `/moderator/*` | CSR | Công cụ Moderator |

### 2.4.2. Backend Layers (Clean Architecture)

| Lớp | Trách nhiệm | Công nghệ |
|-----|-------------|-----------|
| **API Layer** | Nhận HTTP request, routing, middleware (Auth, CORS, Rate Limit) | ASP.NET Core Controllers |
| **Application Layer** | Business logic — Commands (write), Queries (read), validation | MediatR + FluentValidation |
| **Domain Layer** | Entities, Domain rules, Repository interfaces, Value Objects | Pure C# classes |
| **Infrastructure Layer** | Triển khai kết nối DB, cache, email, file storage | EF Core, Redis, Cloudinary, SMTP |
| **SignalR Hubs** | Real-time events — Chat, Notifications, Presence | ASP.NET Core SignalR |

### 2.4.3. CQRS Pattern với MediatR

```
Client Request
     │
     ▼
Controller (nhận DTO)
     │
     ▼ MediatR.Send()
     │
     ├── Command Handler (Write operations)
     │        │
     │        ▼
     │   EF Core → PostgreSQL
     │
     └── Query Handler (Read operations)
              │
              ▼
         Redis Cache → (Cache miss) → EF Core → PostgreSQL
```

---

## 2.6. Chiến lược Real-time với SignalR

### SignalR Hubs

| Hub | Endpoint | Chức năng |
|-----|----------|-----------|
| **ChatHub** | `/hubs/chat` | Gửi/nhận tin nhắn 1-1 và Group; xử lý typing indicator |
| **NotificationHub** | `/hubs/notifications` | Push thông báo Upvote, Comment, Mention theo thời gian thực |
| **PresenceHub** | `/hubs/presence` | Cập nhật trạng thái Online/Offline của người dùng |

### Luồng Chat Real-time (1-1 Message)

```
User A (Browser)                  Server (SignalR)              User B (Browser)
      │                                  │                              │
      │── SendMessage(roomId, msg) ──────►│                              │
      │                                  │── Persist to PostgreSQL       │
      │                                  │── Cache in Redis              │
      │                                  │── BroadcastToRoom() ─────────►│
      │◄── ReceiveMessage(msg) ──────────│                              │
      │                                  │◄── ReceiveMessage(msg) ───────│
```

### SignalR Scale-out với Redis Backplane

```
                    ┌─────────────────────┐
                    │    Redis Backplane   │
                    │  (Pub/Sub Channel)  │
                    └──────────┬──────────┘
                               │
              ┌────────────────┼────────────────┐
              │                │                │
    ┌─────────▼────┐  ┌────────▼─────┐  ┌──────▼───────┐
    │  App Instance│  │ App Instance │  │ App Instance │
    │      #1      │  │     #2       │  │     #3       │
    └──────────────┘  └─────────────┘  └─────────────┘
```

> Khi scale horizontally, Redis Backplane đảm bảo tin nhắn từ User A (kết nối tới Instance #1) vẫn đến được User B (kết nối tới Instance #2).

### Typing Indicator

```csharp
// Client gửi event khi đang gõ
await hubConnection.InvokeAsync("UserTyping", conversationId);

// Server broadcast tới người còn lại (trừ sender)
await Clients.OthersInGroup(conversationId).SendAsync("UserIsTyping", userId);
```

---

## 2.7. Luồng Xác thực (Auth Flow)

### 2.6.1. JWT Authentication Flow

```
Client                          API Server                    Database
  │                                  │                            │
  │── POST /api/auth/login ──────────►│                            │
  │   { email, password }            │── Verify credentials ──────►│
  │                                  │◄── User record ─────────────│
  │                                  │── Generate:                 │
  │                                  │   Access Token (15 min)     │
  │                                  │   Refresh Token (7 days)    │
  │                                  │── Store RefreshToken ───────►│ (Redis)
  │◄── 200 OK ───────────────────────│                            │
  │   { accessToken, refreshToken }  │                            │
  │                                  │                            │
  │── GET /api/posts (Bearer AT) ────►│                            │
  │◄── 200 OK (Data) ────────────────│                            │
  │                                  │                            │
  │── [AT expired] ─────────────────►│◄── 401 Unauthorized         │
  │── POST /api/auth/refresh ─────────►│                            │
  │   { refreshToken }               │── Validate RT in Redis ─────►│
  │◄── { new accessToken } ──────────│                            │
```

### 2.6.2. OAuth2 Flow (Google / GitHub)

```
Client                    Server                    Provider (Google/GitHub)
  │                          │                                │
  │── GET /api/auth/google ──►│                                │
  │◄── Redirect to Google ───│                                │
  │                          │                                │
  │── [User logs in at Provider] ─────────────────────────────►│
  │◄── Redirect with ?code=xxx ───────────────────────────────│
  │                          │                                │
  │── GET /api/auth/callback?code=xxx ──►│                     │
  │                          │── Exchange code for token ─────►│
  │                          │◄── { access_token, profile } ──│
  │                          │── Find/Create User in DB        │
  │                          │── Generate JWT                  │
  │◄── Redirect to FE with JWT ──────────│                     │
```

### 2.6.3. Token Storage Strategy

| Token | Lưu tại Client | Lý do |
|-------|---------------|-------|
| **Access Token** | `Memory` (Zustand store) | Tránh XSS — không lưu localStorage |
| **Refresh Token** | `HttpOnly Cookie` | Tránh XSS — JS không đọc được |

---

## 2.8. Chiến lược Lưu trữ File (Cloud Storage)

### Provider: Cloudinary

| Loại file | Xử lý | CDN URL |
|-----------|-------|---------|
| **Avatar người dùng** | Resize về 200x200px, WebP format | `res.cloudinary.com/.../avatar/` |
| **Ảnh bài viết / bình luận** | Giới hạn 5MB, tự động compress | `res.cloudinary.com/.../posts/` |
| **File gửi qua Chat** | Giới hạn 10MB, kiểm tra MIME type | `res.cloudinary.com/.../chat/` |

### Upload Flow

```
Client                     API Server                   Cloudinary
  │                             │                            │
  │── POST /api/upload ─────────►│                            │
  │   (multipart/form-data)     │── Validate file type/size   │
  │                             │── Upload to Cloudinary ─────►│
  │                             │◄── { secure_url, public_id }│
  │◄── { url: "https://..." } ──│                            │
  │                             │                            │
  │   [Dùng URL này khi tạo Post / gửi Chat message]          │
```

---

## 2.9. Chiến lược Caching & Rate Limiting

### Caching Strategy (Redis)

| Dữ liệu | Cache key pattern | TTL | Lý do |
|---------|-------------------|-----|-------|
| Danh sách Categories | `categories:all` | 1 giờ | Ít thay đổi, đọc nhiều |
| Chi tiết Post | `post:{postId}` | 10 phút | Đọc nhiều, cần fresh data |
| Hot Posts | `posts:hot:{categoryId}` | 5 phút | Tính toán nặng |
| User Profile công khai | `profile:{userId}` | 15 phút | Đọc nhiều |
| Refresh Token | `rt:{userId}:{tokenId}` | 7 ngày | Revocation support |
| Online Status | `presence:{userId}` | 5 phút | Cập nhật liên tục |

### Rate Limiting (ASP.NET Core Rate Limiter)

| Endpoint | Giới hạn | Cửa sổ | Đối tượng |
|----------|---------|--------|-----------|
| `POST /api/auth/login` | 5 request | 1 phút | Per IP |
| `POST /api/auth/register` | 3 request | 1 giờ | Per IP |
| `POST /api/posts` | 10 request | 1 giờ | Per User |
| `POST /api/comments` | 30 request | 1 giờ | Per User |
| `POST /api/chat/messages` | 60 request | 1 phút | Per User |
| `POST /api/upload` | 20 request | 1 giờ | Per User |
| API chung | 200 request | 1 phút | Per User |

---

## 2.9. Kiến trúc Triển khai (Deployment Architecture)

```
Internet
    │
    ▼
┌──────────────────────────────────────────────────────────────┐
│                    Cloudflare (DNS + CDN + DDoS)             │
└─────────────────────────────┬────────────────────────────────┘
                              │ HTTPS (443)
                              ▼
┌──────────────────────────────────────────────────────────────┐
│                    Nginx Reverse Proxy                        │
│   - SSL Termination (Let's Encrypt)                          │
│   - Route: /api/* → ASP.NET Core                            │
│   - Route: /hubs/* → ASP.NET Core (WebSocket upgrade)        │
│   - Route: /* → React SPA (static files)                     │
└──────────┬───────────────────────────────────────────────────┘
           │
    ┌──────▼──────┐
    │  Docker     │
    │  Network    │
    └──────┬──────┘
           │
    ┌──────┴─────────────────────────────┐
    │                                    │
    ▼                                    ▼
┌────────────────┐              ┌────────────────────┐
│ forum-flow-api │              │  forum-flow-web     │
│ (ASP.NET Core) │              │  (Nginx static)     │
│ Port: 5000     │              │  Port: 80           │
└───────┬────────┘              └────────────────────┘
        │
   ┌────┴────────────────────┐
   │                         │
   ▼                         ▼
┌──────────┐         ┌──────────────┐
│PostgreSQL│         │   Redis      │
│Port:5432 │         │  Port: 6379  │
└──────────┘         └──────────────┘
```

### Docker Compose Services

| Service | Image | Port | Mô tả |
|---------|-------|------|-------|
| `forum-flow-api` | Custom (dotnet:8) | 5000 | ASP.NET Core Application |
| `forum-flow-web` | nginx:alpine | 80 | React SPA static files |
| `postgres` | postgres:16 | 5432 | Primary Database |
| `redis` | redis:7-alpine | 6379 | Cache + SignalR Backplane |
| `nginx-proxy` | nginx:latest | 443 | Reverse proxy + SSL |

---

## 2.11. Cấu trúc Thư mục Dự án (Project Structure)

### Backend (ASP.NET Core 8 — Clean Architecture)

```
ForumFlow.Backend/
├── src/
│   ├── ForumFlow.API/                    # Presentation Layer
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── PostsController.cs
│   │   │   ├── CommentsController.cs
│   │   │   ├── ChatController.cs
│   │   │   ├── UsersController.cs
│   │   │   └── AdminController.cs
│   │   ├── Hubs/
│   │   │   ├── ChatHub.cs
│   │   │   ├── NotificationHub.cs
│   │   │   └── PresenceHub.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── RateLimitingMiddleware.cs
│   │   └── Program.cs
│   │
│   ├── ForumFlow.Application/            # Application Layer
│   │   ├── Auth/
│   │   │   ├── Commands/
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   ├── RegisterCommand.cs
│   │   │   │   └── RefreshTokenCommand.cs
│   │   │   └── Queries/
│   │   ├── Forum/
│   │   │   ├── Commands/
│   │   │   │   ├── CreatePostCommand.cs
│   │   │   │   ├── UpdatePostCommand.cs
│   │   │   │   └── DeletePostCommand.cs
│   │   │   └── Queries/
│   │   │       ├── GetPostsQuery.cs
│   │   │       └── GetPostByIdQuery.cs
│   │   ├── Chat/
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   ├── Behaviors/               # MediatR pipeline behaviors
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   └── LoggingBehavior.cs
│   │   │   └── DTOs/
│   │   └── DependencyInjection.cs
│   │
│   ├── ForumFlow.Domain/                 # Domain Layer
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Post.cs
│   │   │   ├── Comment.cs
│   │   │   ├── Category.cs
│   │   │   ├── ChatRoom.cs
│   │   │   ├── Message.cs
│   │   │   └── Notification.cs
│   │   ├── Enums/
│   │   │   ├── UserRole.cs
│   │   │   └── PostStatus.cs
│   │   └── Interfaces/
│   │       ├── IPostRepository.cs
│   │       └── IChatRepository.cs
│   │
│   └── ForumFlow.Infrastructure/         # Infrastructure Layer
│       ├── Persistence/
│       │   ├── ApplicationDbContext.cs
│       │   ├── Configurations/           # EF Core entity configs
│       │   ├── Repositories/
│       │   └── Migrations/
│       ├── Cache/
│       │   └── RedisCacheService.cs
│       ├── Identity/
│       │   └── JwtTokenService.cs
│       ├── Storage/
│       │   └── CloudinaryService.cs
│       ├── Email/
│       │   └── EmailService.cs
│       └── DependencyInjection.cs
│
└── tests/
    ├── ForumFlow.UnitTests/
    └── ForumFlow.IntegrationTests/
```

### Frontend (Next.js 15 — App Router)

```
forum-flow-web/
├── public/                               # Static assets (favicon, og-image...)
├── app/                                  # Next.js App Router (pages & layouts)
│   ├── layout.tsx                        # Root layout (font, theme, providers)
│   ├── page.tsx                          # / — Trang chủ (SSG)
│   ├── (forum)/                          # Route group — các trang có SEO
│   │   ├── c/
│   │   │   └── [slug]/
│   │   │       └── page.tsx              # /c/[slug] — Category posts (ISR)
│   │   ├── p/
│   │   │   └── [slug]/
│   │   │       └── page.tsx              # /p/[slug] — Post detail (SSR)
│   │   ├── u/
│   │   │   └── [username]/
│   │   │       └── page.tsx              # /u/[username] — Profile (SSR)
│   │   └── search/
│   │       └── page.tsx                  # /search — Kết quả tìm kiếm (SSR)
│   ├── (auth)/                           # Route group — Auth pages (CSR)
│   │   ├── login/page.tsx
│   │   ├── register/page.tsx
│   │   └── forgot-password/page.tsx
│   ├── (app)/                            # Route group — Authenticated CSR
│   │   ├── chat/
│   │   │   └── page.tsx                  # /chat — Chat interface
│   │   ├── notifications/page.tsx
│   │   ├── settings/page.tsx
│   │   ├── posts/
│   │   │   └── new/page.tsx              # Tạo bài viết mới
│   │   ├── admin/
│   │   │   ├── page.tsx                  # Admin dashboard
│   │   │   ├── users/page.tsx
│   │   │   └── categories/page.tsx
│   │   └── moderator/
│   │       └── reports/page.tsx
│   └── api/                              # Next.js API routes (nếu cần)
│       └── auth/
│           └── [...nextauth]/route.ts    # NextAuth callback (OAuth)
├── components/                           # Shared UI components
│   ├── ui/                               # Shadcn/UI base components
│   ├── forum/
│   │   ├── PostCard.tsx
│   │   ├── PostEditor.tsx
│   │   └── CommentThread.tsx
│   ├── chat/
│   │   ├── ChatSidebar.tsx
│   │   ├── MessageBubble.tsx
│   │   └── TypingIndicator.tsx
│   ├── layout/
│   │   ├── Navbar.tsx
│   │   └── Sidebar.tsx
│   └── shared/
│       ├── Avatar.tsx
│       └── NotificationBell.tsx
├── lib/                                  # Utilities & config
│   ├── api.ts                            # Axios instance + JWT interceptors
│   ├── signalr.ts                        # SignalR connection manager
│   ├── queryClient.ts                    # TanStack Query config
│   └── utils.ts
├── hooks/                                # Custom React hooks
│   ├── useSignalR.ts
│   ├── useAuth.ts
│   └── useNotifications.ts
├── store/                                # Zustand stores
│   ├── authStore.ts
│   └── chatStore.ts
├── types/                                # TypeScript type definitions
├── styles/
│   └── globals.css
├── next.config.ts                        # Next.js configuration
├── tailwind.config.ts
├── tsconfig.json
└── package.json
```

---

## Tổng kết Tech Stack (Chốt — v1.1)

```
┌─────────────────────────────────────────────────────────────┐
│                    FORUM-FLOW TECH STACK                     │
├─────────────────────────────────────────────────────────────┤
│  Frontend     │ Next.js 15 (App Router) + TypeScript         │
│               │ React 19 + TanStack Query + Zustand          │
│               │ SignalR Client + Shadcn/UI + Tailwind CSS     │
│               │ Hybrid Rendering: SSR / SSG / ISR / CSR      │
├─────────────────────────────────────────────────────────────┤
│  Backend      │ ASP.NET Core 8 (LTS)                         │
│               │ SignalR (Real-time) + MediatR (CQRS)         │
│               │ EF Core 8 + FluentValidation + Hangfire      │
├─────────────────────────────────────────────────────────────┤
│  Auth         │ JWT (Access + Refresh) + OAuth2              │
│               │ ASP.NET Identity + Policy-based RBAC         │
├─────────────────────────────────────────────────────────────┤
│  Database     │ PostgreSQL 16 (Primary)                      │
│               │ Redis 7 (Cache + SignalR Backplane)          │
├─────────────────────────────────────────────────────────────┤
│  Storage      │ Cloudinary (ảnh, avatar, file chat)          │
├─────────────────────────────────────────────────────────────┤
│  SEO          │ Next.js Metadata API + next-seo              │
│               │ Open Graph, JSON-LD, Sitemap tự động         │
├─────────────────────────────────────────────────────────────┤
│  DevOps       │ Docker + Docker Compose + Nginx              │
│               │ GitHub Actions (CI/CD)                       │
└─────────────────────────────────────────────────────────────┘
```

---

*Tài liệu này là một phần của bộ Detailed Specification cho dự án **Forum-Flow**.*  
*Chương tiếp theo: **Chương III — Thiết kế Cơ sở Dữ liệu (Database Design & ERD)***
