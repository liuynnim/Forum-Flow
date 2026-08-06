# CHƯƠNG IV: ĐẶC TẢ API
## (API SPECIFICATION — REST & SignalR)

> **Dự án:** Forum-Flow — Forum / Community Platform kết hợp Chat trực tiếp  
> **Phiên bản tài liệu:** 1.0.0  
> **Ngày tạo:** 2026-08-02  
> **Base URL:** `https://api.forumflow.io/api/v1`  
> **Trạng thái:** Draft

---

## Mục lục

- [4.1. Quy ước Chung (API Conventions)](#41-quy-ước-chung-api-conventions)
- [4.2. Authentication & Authorization](#42-authentication--authorization-endpoints)
- [4.3. Users & Profile](#43-users--profile-endpoints)
- [4.4. Categories & Tags](#44-categories--tags-endpoints)
- [4.5. Posts (Forum)](#45-posts-forum-endpoints)
- [4.6. Comments](#46-comments-endpoints)
- [4.7. Chat](#47-chat-endpoints)
- [4.8. Notifications](#48-notifications-endpoints)
- [4.9. Uploads](#49-uploads-endpoints)
- [4.10. Moderator Actions](#410-moderator-actions-endpoints)
- [4.11. Admin](#411-admin-endpoints)
- [4.12. SignalR Hubs (Real-time)](#412-signalr-hubs-real-time)
- [4.13. Tổng hợp Endpoints](#413-tổng-hợp-endpoints)

---

## 4.1. Quy ước Chung (API Conventions)

### Base URL & Versioning

```
Production : https://api.forumflow.io/api/v1
Development: http://localhost:5000/api/v1
```

### Authentication Header

Tất cả endpoint yêu cầu xác thực cần gửi kèm JWT Access Token:

```http
Authorization: Bearer <access_token>
```

### Chuẩn Response Envelope

Mọi response đều bọc trong một envelope thống nhất:

```json
// ✅ Success Response (2xx)
{
  "success": true,
  "data": { ... },          // Dữ liệu trả về
  "message": "OK",
  "meta": {                  // Chỉ có trong paginated response
    "page": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8
  }
}

// ❌ Error Response (4xx / 5xx)
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Dữ liệu không hợp lệ",
    "details": [
      { "field": "email", "message": "Email không đúng định dạng" }
    ]
  }
}
```

### HTTP Status Codes

| Code | Ý nghĩa |
|------|---------|
| `200 OK` | Thành công |
| `201 Created` | Tạo mới thành công |
| `204 No Content` | Xóa / Xử lý thành công, không có dữ liệu trả về |
| `400 Bad Request` | Dữ liệu đầu vào không hợp lệ |
| `401 Unauthorized` | Chưa xác thực hoặc token hết hạn |
| `403 Forbidden` | Đã xác thực nhưng không đủ quyền |
| `404 Not Found` | Tài nguyên không tồn tại |
| `409 Conflict` | Xung đột dữ liệu (email đã tồn tại, v.v.) |
| `422 Unprocessable Entity` | Validation lỗi business logic |
| `429 Too Many Requests` | Rate limit exceeded |
| `500 Internal Server Error` | Lỗi server |

### Error Codes Chuẩn

| Code | Mô tả |
|------|-------|
| `VALIDATION_ERROR` | Dữ liệu đầu vào vi phạm quy tắc |
| `UNAUTHORIZED` | Token thiếu hoặc không hợp lệ |
| `TOKEN_EXPIRED` | Access token đã hết hạn |
| `FORBIDDEN` | Không đủ quyền |
| `NOT_FOUND` | Không tìm thấy resource |
| `CONFLICT` | Tài nguyên đã tồn tại |
| `RATE_LIMIT_EXCEEDED` | Vượt quá giới hạn request |
| `ACCOUNT_BANNED` | Tài khoản bị khóa |
| `EMAIL_NOT_VERIFIED` | Email chưa xác minh |

### Pagination Query Parameters

```
GET /api/v1/posts?page=1&pageSize=20&sort=createdAt&order=desc
```

| Param | Default | Mô tả |
|-------|---------|-------|
| `page` | 1 | Trang hiện tại |
| `pageSize` | 20 | Số item mỗi trang (max: 50) |
| `sort` | `createdAt` | Trường sắp xếp |
| `order` | `desc` | `asc` hoặc `desc` |

### Ký hiệu Phân quyền

| Ký hiệu | Nghĩa |
|---------|-------|
| 🌐 | Public — không cần đăng nhập |
| 🔑 | Member — cần đăng nhập |
| 🛡️ | Moderator trở lên |
| 👑 | Admin only |

---

## 4.2. Authentication & Authorization Endpoints

### 4.2.1. Đăng ký tài khoản

```http
POST /auth/register
```
🌐 Public

**Request Body:**
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "displayName": "John Doe"
}
```

**Validation Rules:**
- `username`: 3–50 ký tự, chỉ `a-z`, `0-9`, `_`, `-`; chưa tồn tại trong DB
- `email`: định dạng email hợp lệ; chưa tồn tại trong DB
- `password`: tối thiểu 8 ký tự, có chữ hoa, chữ thường và số
- `displayName`: 2–100 ký tự

**Response `201 Created`:**
```json
{
  "success": true,
  "data": {
    "message": "Đăng ký thành công. Vui lòng kiểm tra email để xác minh tài khoản."
  }
}
```

**Errors:** `409` (email/username đã tồn tại), `400` (validation)

---

### 4.2.2. Xác minh Email

```http
GET /auth/verify-email?token={token}
```
🌐 Public

**Response `200 OK`:**
```json
{ "success": true, "data": { "message": "Email đã được xác minh thành công." } }
```

---

### 4.2.3. Đăng nhập

```http
POST /auth/login
```
🌐 Public

**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

**Response `200 OK`:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 900,
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "username": "johndoe",
      "email": "john@example.com",
      "displayName": "John Doe",
      "avatarUrl": "https://res.cloudinary.com/...",
      "role": "MEMBER"
    }
  }
}
```
> Refresh Token được gửi qua `HttpOnly Cookie` (không xuất hiện trong response body).

**Errors:** `401` (sai credentials), `403` (tài khoản bị ban), `403` (chưa verify email)

---

### 4.2.4. Làm mới Access Token

```http
POST /auth/refresh
```
🌐 Public — Đọc Refresh Token từ `HttpOnly Cookie`

**Response `200 OK`:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 900
  }
}
```

---

### 4.2.5. Đăng xuất

```http
POST /auth/logout
```
🔑 Member

> Revoke Refresh Token hiện tại. Cookie bị xóa.

**Response `204 No Content`**

---

### 4.2.6. OAuth2 — Khởi tạo

```http
GET /auth/google      → Redirect to Google OAuth2
GET /auth/github      → Redirect to GitHub OAuth2
```
🌐 Public

---

### 4.2.7. OAuth2 — Callback

```http
GET /auth/google/callback?code={code}&state={state}
GET /auth/github/callback?code={code}&state={state}
```
🌐 Public — Server xử lý, redirect về FE kèm token

---

### 4.2.8. Yêu cầu Đặt lại Mật khẩu

```http
POST /auth/forgot-password
```
🌐 Public

**Request Body:**
```json
{ "email": "john@example.com" }
```

**Response `200 OK`:**
```json
{ "success": true, "data": { "message": "Link đặt lại mật khẩu đã được gửi." } }
```
> Luôn trả về 200 dù email có tồn tại hay không (tránh User Enumeration).

---

### 4.2.9. Đặt lại Mật khẩu

```http
POST /auth/reset-password
```
🌐 Public

**Request Body:**
```json
{
  "token": "reset-token-from-email",
  "newPassword": "NewSecurePass456!"
}
```

**Response `200 OK`**

---

## 4.3. Users & Profile Endpoints

### 4.3.1. Xem Profile công khai

```http
GET /users/{username}
```
🌐 Public

**Response `200 OK`:**
```json
{
  "success": true,
  "data": {
    "id": "550e8400...",
    "username": "johndoe",
    "displayName": "John Doe",
    "avatarUrl": "https://...",
    "bio": "Full-stack developer",
    "websiteUrl": "https://johndoe.dev",
    "githubUrl": "https://github.com/johndoe",
    "role": "MEMBER",
    "postCount": 42,
    "createdAt": "2026-01-15T10:30:00Z",
    "lastSeenAt": "2026-08-02T09:00:00Z"
  }
}
```

---

### 4.3.2. Xem Profile bản thân

```http
GET /users/me
```
🔑 Member

> Trả về đầy đủ thông tin kể cả email, sessions (không trả cho người khác).

---

### 4.3.3. Cập nhật Profile

```http
PATCH /users/me
```
🔑 Member

**Request Body (tất cả optional):**
```json
{
  "displayName": "John Updated",
  "bio": "Backend developer @ForumFlow",
  "websiteUrl": "https://johndoe.dev",
  "githubUrl": "https://github.com/johndoe"
}
```

**Response `200 OK`:** Trả về profile đã cập nhật.

---

### 4.3.4. Đổi Mật khẩu

```http
PUT /users/me/password
```
🔑 Member

**Request Body:**
```json
{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass456!"
}
```

---

### 4.3.5. Xem Active Sessions

```http
GET /users/me/sessions
```
🔑 Member

**Response `200 OK`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "token-id-1",
      "deviceInfo": "Chrome 126.0 / Windows 11",
      "ipAddress": "103.x.x.x",
      "createdAt": "2026-08-01T08:00:00Z",
      "isCurrent": true
    }
  ]
}
```

---

### 4.3.6. Thu hồi Session

```http
DELETE /users/me/sessions/{sessionId}
```
🔑 Member — Đăng xuất một phiên cụ thể từ xa.

**Response `204 No Content`**

---

### 4.3.7. Bài viết đã Bookmark

```http
GET /users/me/bookmarks?page=1&pageSize=20
```
🔑 Member

---

## 4.4. Categories & Tags Endpoints

### 4.4.1. Danh sách Categories

```http
GET /categories
```
🌐 Public

**Response `200 OK`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "name": "Lập trình",
      "slug": "lap-trinh",
      "description": "Thảo luận về các ngôn ngữ và công nghệ lập trình",
      "icon": "code",
      "color": "#3B82F6",
      "displayOrder": 1,
      "postCount": 234
    }
  ]
}
```

---

### 4.4.2. Chi tiết Category

```http
GET /categories/{slug}
```
🌐 Public

---

### 4.4.3. Danh sách Tags

```http
GET /tags?q={search}&page=1&pageSize=20
```
🌐 Public — Hỗ trợ tìm kiếm tag theo tên.

---

### 4.4.4. Tạo Category mới 👑

```http
POST /categories
```
👑 Admin

**Request Body:**
```json
{
  "name": "Thiết kế UI/UX",
  "slug": "thiet-ke-ui-ux",
  "description": "Thảo luận về thiết kế giao diện và trải nghiệm người dùng",
  "icon": "palette",
  "color": "#8B5CF6",
  "displayOrder": 5
}
```

---

### 4.4.5. Cập nhật Category 👑

```http
PUT /categories/{id}
```
👑 Admin

---

### 4.4.6. Xóa Category 👑

```http
DELETE /categories/{id}
```
👑 Admin

**Errors:** `409` nếu category vẫn còn bài viết.

---

### 4.4.7. Sắp xếp Categories 👑

```http
PATCH /categories/reorder
```
👑 Admin

**Request Body:**
```json
{
  "order": [
    { "id": "uuid-1", "displayOrder": 1 },
    { "id": "uuid-2", "displayOrder": 2 }
  ]
}
```

---

## 4.5. Posts (Forum) Endpoints

### 4.5.1. Danh sách Posts

```http
GET /posts?categorySlug=lap-trinh&tag=dotnet&sort=hot&page=1&pageSize=20
```
🌐 Public

**Query Parameters:**

| Param | Mô tả |
|-------|-------|
| `categorySlug` | Lọc theo chuyên mục |
| `tag` | Lọc theo tag (slug) |
| `sort` | `newest` \| `hot` \| `top` \| `unanswered` |
| `q` | Full-text search |

**Response `200 OK`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "title": "Hướng dẫn dùng SignalR trong .NET 8",
      "slug": "huong-dan-dung-signalr-trong-dotnet-8-x7k2p",
      "excerpt": "SignalR là thư viện cho phép...",
      "author": {
        "id": "uuid",
        "username": "johndoe",
        "displayName": "John Doe",
        "avatarUrl": "https://..."
      },
      "category": { "id": "uuid", "name": "Lập trình", "slug": "lap-trinh" },
      "tags": [{ "id": "uuid", "name": ".NET", "slug": "dotnet" }],
      "voteScore": 42,
      "commentCount": 15,
      "viewCount": 1200,
      "isPinned": false,
      "status": "PUBLISHED",
      "createdAt": "2026-07-20T10:00:00Z",
      "updatedAt": "2026-07-21T08:30:00Z",
      "userVote": 1         // null | 1 | -1 (chỉ trả nếu đã đăng nhập)
    }
  ],
  "meta": { "page": 1, "pageSize": 20, "totalItems": 234, "totalPages": 12 }
}
```

---

### 4.5.2. Chi tiết Post

```http
GET /posts/{slug}
```
🌐 Public — Tăng `view_count` mỗi lần gọi (debounce server-side).

**Response `200 OK`:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "title": "Hướng dẫn dùng SignalR trong .NET 8",
    "slug": "...",
    "content": "# SignalR\n\nSignalR là...",   // Markdown source
    "contentHtml": "<h1>SignalR</h1><p>...",   // Rendered HTML
    "author": { ... },
    "category": { ... },
    "tags": [ ... ],
    "voteScore": 42,
    "commentCount": 15,
    "viewCount": 1201,
    "isPinned": false,
    "isBookmarked": false,   // Chỉ trả nếu đã đăng nhập
    "userVote": null,
    "status": "PUBLISHED",
    "editHistory": [         // Chỉ trả về cho owner/mod/admin
      { "editedAt": "2026-07-21T08:30:00Z", "editorUsername": "johndoe" }
    ],
    "createdAt": "2026-07-20T10:00:00Z",
    "updatedAt": "2026-07-21T08:30:00Z"
  }
}
```

---

### 4.5.3. Tạo Post mới

```http
POST /posts
```
🔑 Member

**Request Body:**
```json
{
  "title": "Hướng dẫn dùng SignalR trong .NET 8",
  "content": "# SignalR\n\nSignalR là thư viện...",
  "categoryId": "uuid",
  "tagIds": ["uuid-tag-1", "uuid-tag-2"],
  "status": "PUBLISHED"   // "DRAFT" | "PUBLISHED"
}
```

**Response `201 Created`:** Trả về post đầy đủ.

---

### 4.5.4. Cập nhật Post

```http
PUT /posts/{id}
```
🔑 Member (chỉ owner) | 🛡️ Moderator | 👑 Admin

**Request Body:** Tương tự Create, thêm `editReason` optional.

---

### 4.5.5. Xóa Post

```http
DELETE /posts/{id}
```
🔑 Member (chỉ owner) | 🛡️ Moderator | 👑 Admin

**Response `204 No Content`** — Soft delete.

---

### 4.5.6. Vote Post

```http
POST /posts/{id}/vote
```
🔑 Member

**Request Body:**
```json
{ "voteType": 1 }   // 1 = upvote, -1 = downvote, 0 = xóa vote
```

**Response `200 OK`:**
```json
{
  "success": true,
  "data": { "voteScore": 43, "userVote": 1 }
}
```

---

### 4.5.7. Bookmark Post

```http
POST /posts/{id}/bookmark
```
🔑 Member — Toggle: lần đầu thêm, lần sau xóa.

**Response `200 OK`:**
```json
{ "success": true, "data": { "isBookmarked": true } }
```

---

### 4.5.8. Báo cáo Post

```http
POST /posts/{id}/report
```
🔑 Member

**Request Body:**
```json
{
  "reason": "spam",    // "spam"|"offensive"|"misinformation"|"other"
  "description": "Bài viết quảng cáo sản phẩm không liên quan"
}
```

---

### 4.5.9. Lịch sử Chỉnh sửa Post

```http
GET /posts/{id}/history
```
🔑 Owner | 🛡️ Moderator | 👑 Admin

---

## 4.6. Comments Endpoints

### 4.6.1. Danh sách Comments của Post

```http
GET /posts/{postId}/comments?page=1&pageSize=20&sort=top
```
🌐 Public

**Response `200 OK`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "content": "Bài viết rất hay, cảm ơn tác giả!",
      "author": { "id": "uuid", "username": "alice", "displayName": "Alice", "avatarUrl": "..." },
      "voteScore": 5,
      "replyCount": 3,
      "depth": 0,
      "parentCommentId": null,
      "userVote": null,
      "isDeleted": false,
      "createdAt": "2026-07-20T11:00:00Z",
      "replies": [           // Trả về 3 replies đầu tiên
        {
          "id": "uuid-reply",
          "content": "@alice Cảm ơn bạn!",
          "author": { ... },
          "depth": 1,
          "parentCommentId": "uuid",
          ...
        }
      ]
    }
  ],
  "meta": { ... }
}
```

---

### 4.6.2. Load thêm Replies

```http
GET /comments/{commentId}/replies?page=1&pageSize=10
```
🌐 Public

---

### 4.6.3. Tạo Comment

```http
POST /posts/{postId}/comments
```
🔑 Member

**Request Body:**
```json
{
  "content": "Bài viết rất hay! Mình cũng đang tìm hiểu về SignalR.",
  "parentCommentId": null      // null = root comment, có ID = reply
}
```

**Response `201 Created`:**
```json
{
  "success": true,
  "data": {
    "id": "uuid-new-comment",
    "content": "...",
    "author": { ... },
    "depth": 0,
    "parentCommentId": null,
    "voteScore": 0,
    "createdAt": "2026-08-02T10:00:00Z"
  }
}
```

---

### 4.6.4. Cập nhật Comment

```http
PUT /comments/{id}
```
🔑 Owner only

**Request Body:**
```json
{ "content": "Nội dung đã chỉnh sửa..." }
```

---

### 4.6.5. Xóa Comment

```http
DELETE /comments/{id}
```
🔑 Owner | 🛡️ Moderator | 👑 Admin

> Soft delete: nội dung bị xóa nhưng cấu trúc cây giữ nguyên. Replies vẫn hiển thị.

---

### 4.6.6. Vote Comment

```http
POST /comments/{id}/vote
```
🔑 Member

**Request Body:**
```json
{ "voteType": 1 }    // 1 | -1 | 0
```

---

### 4.6.7. Báo cáo Comment

```http
POST /comments/{id}/report
```
🔑 Member

---

## 4.7. Chat Endpoints

### 4.7.1. Danh sách Cuộc trò chuyện (Rooms)

```http
GET /chat/rooms?page=1&pageSize=20
```
🔑 Member — Trả về các room user đang tham gia, sắp xếp theo `last_message_at`.

**Response `200 OK`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid-room",
      "name": "Team Frontend",
      "type": "GROUP",
      "avatarUrl": "https://...",
      "lastMessagePreview": "Hôm nay deploy lúc mấy giờ?",
      "lastMessageAt": "2026-08-02T09:45:00Z",
      "unreadCount": 3,
      "members": [
        { "id": "uuid", "username": "alice", "avatarUrl": "...", "isOnline": true }
      ]
    }
  ]
}
```

---

### 4.7.2. Tạo Group Room mới

```http
POST /chat/rooms
```
🔑 Member

**Request Body:**
```json
{
  "name": "Team Backend",
  "type": "GROUP",
  "memberIds": ["uuid-user-2", "uuid-user-3"]
}
```

---

### 4.7.3. Tạo / Lấy Direct Message Room

```http
POST /chat/rooms/direct
```
🔑 Member — Idempotent: tìm room DM đã có với user kia, nếu chưa có thì tạo mới.

**Request Body:**
```json
{ "targetUserId": "uuid-target-user" }
```

**Response `200 OK` hoặc `201 Created`:** Trả về room object.

---

### 4.7.4. Chi tiết Room

```http
GET /chat/rooms/{roomId}
```
🔑 Member (phải là thành viên của room)

---

### 4.7.5. Thêm Thành viên vào Group

```http
POST /chat/rooms/{roomId}/members
```
🔑 Member (Group Admin)

**Request Body:**
```json
{ "userIds": ["uuid-user-4"] }
```

---

### 4.7.6. Rời Room

```http
DELETE /chat/rooms/{roomId}/members/me
```
🔑 Member

---

### 4.7.7. Lịch sử Tin nhắn

```http
GET /chat/rooms/{roomId}/messages?before={messageId}&limit=50
```
🔑 Member (phải là thành viên)

> Cursor-based pagination (không dùng page/offset) để tránh bỏ sót tin nhắn mới.

**Response `200 OK`:**
```json
{
  "success": true,
  "data": {
    "messages": [
      {
        "id": "uuid-msg",
        "sender": { "id": "uuid", "username": "alice", "avatarUrl": "..." },
        "content": "Deploy lúc 10 giờ tối nha mọi người",
        "type": "TEXT",
        "fileUrl": null,
        "isDeleted": false,
        "createdAt": "2026-08-02T09:45:00Z"
      }
    ],
    "hasMore": true,
    "oldestMessageId": "uuid-oldest"
  }
}
```

---

### 4.7.8. Đánh dấu đã đọc

```http
POST /chat/rooms/{roomId}/read
```
🔑 Member — Cập nhật `last_read_at` của user trong room.

**Response `204 No Content`**

---

### 4.7.9. Tìm kiếm User để nhắn tin

```http
GET /users/search?q={query}&limit=10
```
🔑 Member

**Response `200 OK`:**
```json
{
  "success": true,
  "data": [
    { "id": "uuid", "username": "alice", "displayName": "Alice", "avatarUrl": "...", "isOnline": true }
  ]
}
```

---

## 4.8. Notifications Endpoints

### 4.8.1. Danh sách Thông báo

```http
GET /notifications?isRead=false&page=1&pageSize=20
```
🔑 Member

**Response `200 OK`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid-notif",
      "type": "POST_UPVOTED",
      "sender": { "id": "uuid", "username": "bob", "avatarUrl": "..." },
      "data": {
        "postTitle": "Hướng dẫn dùng SignalR",
        "postSlug": "huong-dan-dung-signalr-x7k2p"
      },
      "isRead": false,
      "createdAt": "2026-08-02T09:00:00Z"
    }
  ],
  "meta": { "page": 1, "pageSize": 20, "totalItems": 12, "totalPages": 1 }
}
```

---

### 4.8.2. Số thông báo chưa đọc

```http
GET /notifications/unread-count
```
🔑 Member

**Response `200 OK`:**
```json
{ "success": true, "data": { "count": 5 } }
```

---

### 4.8.3. Đánh dấu đã đọc

```http
PATCH /notifications/read
```
🔑 Member

**Request Body:**
```json
{
  "notificationIds": ["uuid-1", "uuid-2"],  // Danh sách cụ thể
  "markAll": false                           // true = đánh dấu tất cả
}
```

---

### 4.8.4. Xóa Thông báo

```http
DELETE /notifications/{id}
```
🔑 Member

---

## 4.9. Uploads Endpoints

### 4.9.1. Upload File / Ảnh

```http
POST /uploads
```
🔑 Member

**Request:** `multipart/form-data`

| Field | Mô tả |
|-------|-------|
| `file` | File upload (ảnh/file) |
| `purpose` | `avatar` \| `post` \| `chat` |

**Giới hạn:**

| Purpose | Max size | Allowed types |
|---------|---------|---------------|
| `avatar` | 2 MB | `image/jpeg`, `image/png`, `image/webp` |
| `post` | 5 MB | `image/*` |
| `chat` | 10 MB | `image/*`, `application/pdf`, `text/*` |

**Response `200 OK`:**
```json
{
  "success": true,
  "data": {
    "url": "https://res.cloudinary.com/forumflow/image/upload/v1234/avatar/johndoe.webp",
    "publicId": "avatar/johndoe",
    "width": 200,
    "height": 200,
    "format": "webp",
    "bytes": 45678
  }
}
```

---

## 4.10. Moderator Actions Endpoints

### 4.10.1. Ghim / Bỏ ghim Post

```http
PATCH /moderator/posts/{id}/pin
```
🛡️ Moderator

**Request Body:**
```json
{ "isPinned": true }
```

---

### 4.10.2. Khóa / Mở khóa Post

```http
PATCH /moderator/posts/{id}/lock
```
🛡️ Moderator

**Request Body:**
```json
{ "isLocked": true }
```

---

### 4.10.3. Chuyển Post sang Category khác

```http
PATCH /moderator/posts/{id}/move
```
🛡️ Moderator

**Request Body:**
```json
{ "categoryId": "uuid-target-category" }
```

---

### 4.10.4. Cập nhật Tags của Post

```http
PATCH /moderator/posts/{id}/tags
```
🛡️ Moderator

**Request Body:**
```json
{ "tagIds": ["uuid-tag-1", "uuid-tag-2"] }
```

---

### 4.10.5. Danh sách Báo cáo chờ xử lý

```http
GET /moderator/reports?status=PENDING&contentType=post&page=1&pageSize=20
```
🛡️ Moderator

**Response `200 OK`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid-report",
      "reporter": { "id": "uuid", "username": "user123" },
      "contentType": "post",
      "contentId": "uuid-post",
      "contentPreview": { "title": "...", "excerpt": "..." },
      "reason": "spam",
      "description": "Quảng cáo sản phẩm...",
      "status": "PENDING",
      "createdAt": "2026-08-01T20:00:00Z"
    }
  ],
  "meta": { ... }
}
```

---

### 4.10.6. Xử lý Báo cáo

```http
PATCH /moderator/reports/{id}
```
🛡️ Moderator

**Request Body:**
```json
{
  "action": "APPROVE",        // "APPROVE" | "REJECT"
  "resolutionNote": "Đã xóa bài viết spam",
  "deleteContent": true       // Chỉ có hiệu lực khi action = "APPROVE"
}
```

---

## 4.11. Admin Endpoints

### 4.11.1. Dashboard Metrics

```http
GET /admin/dashboard
```
👑 Admin

**Response `200 OK`:**
```json
{
  "success": true,
  "data": {
    "overview": {
      "totalUsers": 12450,
      "totalPosts": 45230,
      "totalMessages": 2340000,
      "activeUsersToday": 342
    },
    "trends": {
      "newUsersLast7Days": [
        { "date": "2026-07-27", "count": 45 },
        { "date": "2026-07-28", "count": 52 }
      ],
      "postsLast7Days": [
        { "date": "2026-07-27", "count": 120 }
      ]
    },
    "rateLimiting": {
      "blockedRequestsToday": 234,
      "topOffenders": [
        { "ipAddress": "x.x.x.x", "count": 50 }
      ]
    }
  }
}
```

---

### 4.11.2. Danh sách Users

```http
GET /admin/users?q={search}&role=MEMBER&isBanned=false&page=1&pageSize=20
```
👑 Admin

---

### 4.11.3. Chi tiết User

```http
GET /admin/users/{id}
```
👑 Admin — Trả về đầy đủ thông tin kể cả sessions, report history.

---

### 4.11.4. Phân quyền User

```http
PATCH /admin/users/{id}/role
```
👑 Admin

**Request Body:**
```json
{
  "role": "MODERATOR",
  "categoryIds": ["uuid-cat-1", "uuid-cat-2"]   // Bắt buộc nếu role = MODERATOR
}
```

---

### 4.11.5. Ban / Unban User

```http
PATCH /admin/users/{id}/ban
```
👑 Admin

**Request Body:**
```json
{
  "isBanned": true,
  "banReason": "Spam liên tục sau nhiều cảnh báo",
  "banUntil": null          // null = vĩnh viễn, hoặc ISO8601 datetime
}
```

---

### 4.11.6. Quản lý Tags

```http
GET    /admin/tags                  # Danh sách tất cả tags
POST   /admin/tags                  # Tạo tag mới
PUT    /admin/tags/{id}             # Cập nhật tag
DELETE /admin/tags/{id}             # Xóa tag
POST   /admin/tags/merge            # Hợp nhất 2 tags
```
👑 Admin

**Merge Tags Request:**
```json
{
  "sourceTagId": "uuid-tag-dotnet-core",
  "targetTagId": "uuid-tag-dotnet"       // Giữ tag này, xóa source
}
```

---

### 4.11.7. Rate Limiting Audit Log

```http
GET /admin/rate-limit/logs?page=1&pageSize=50
```
👑 Admin

---

## 4.12. SignalR Hubs (Real-time)

### Kết nối SignalR

```javascript
// Frontend — Khởi tạo kết nối
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://api.forumflow.io/hubs/chat", {
    accessTokenFactory: () => store.getState().accessToken
  })
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Warning)
  .build();

await connection.start();
```

---

### 4.12.1. ChatHub — `/hubs/chat`

🔑 Member — Xử lý tin nhắn thời gian thực.

#### Client → Server (Invoke Methods)

| Method | Params | Mô tả |
|--------|--------|-------|
| `JoinRoom` | `roomId: string` | Tham gia vào SignalR group của room |
| `LeaveRoom` | `roomId: string` | Rời SignalR group của room |
| `SendMessage` | `roomId: string, content: string, type: string` | Gửi tin nhắn text |
| `SendFileMessage` | `roomId: string, fileUrl: string, fileName: string, fileSize: number, mimeType: string` | Gửi tin nhắn file |
| `StartTyping` | `roomId: string` | Bắt đầu gõ |
| `StopTyping` | `roomId: string` | Dừng gõ |
| `DeleteMessage` | `messageId: string` | Xóa tin nhắn của mình |

```typescript
// Ví dụ gửi tin nhắn
await connection.invoke("SendMessage", roomId, "Hello everyone!", "TEXT");

// Ví dụ typing indicator
await connection.invoke("StartTyping", roomId);
```

#### Server → Client (On Methods)

| Method | Payload | Mô tả |
|--------|---------|-------|
| `ReceiveMessage` | `MessageDto` | Nhận tin nhắn mới |
| `MessageDeleted` | `{ messageId, roomId }` | Tin nhắn bị xóa |
| `UserTyping` | `{ userId, username, roomId }` | Ai đó đang gõ |
| `UserStoppedTyping` | `{ userId, roomId }` | Dừng gõ |
| `RoomUpdated` | `ChatRoomDto` | Room được đổi tên/ảnh |
| `MemberJoined` | `{ roomId, user: UserDto }` | Thành viên mới vào room |
| `MemberLeft` | `{ roomId, userId }` | Thành viên rời room |

```typescript
// Ví dụ lắng nghe tin nhắn
connection.on("ReceiveMessage", (message: MessageDto) => {
  dispatch(addMessage(message));
});

// Ví dụ lắng nghe typing
connection.on("UserTyping", ({ userId, username, roomId }) => {
  dispatch(setTypingUser({ roomId, userId, username }));
});
```

**MessageDto:**
```typescript
interface MessageDto {
  id: string;
  roomId: string;
  sender: {
    id: string;
    username: string;
    displayName: string;
    avatarUrl: string;
  };
  content: string | null;
  type: "TEXT" | "IMAGE" | "FILE";
  fileUrl: string | null;
  fileName: string | null;
  fileSize: number | null;
  createdAt: string;    // ISO8601
}
```

---

### 4.12.2. NotificationHub — `/hubs/notifications`

🔑 Member — Nhận thông báo thời gian thực.

#### Server → Client (On Methods)

| Method | Payload | Mô tả |
|--------|---------|-------|
| `ReceiveNotification` | `NotificationDto` | Nhận thông báo mới |
| `UnreadCountUpdated` | `{ count: number }` | Cập nhật số thông báo chưa đọc |

```typescript
connection.on("ReceiveNotification", (notification: NotificationDto) => {
  toast.show(notification);
  dispatch(incrementUnreadCount());
});
```

**NotificationDto:**
```typescript
interface NotificationDto {
  id: string;
  type: "POST_UPVOTED" | "POST_COMMENTED" | "COMMENT_REPLIED" 
      | "USER_MENTIONED" | "NEW_MESSAGE" | "SYSTEM";
  sender: { id: string; username: string; avatarUrl: string } | null;
  data: {
    postTitle?: string;
    postSlug?: string;
    commentPreview?: string;
    roomName?: string;
    messagePreview?: string;
  };
  createdAt: string;
}
```

---

### 4.12.3. PresenceHub — `/hubs/presence`

🔑 Member — Theo dõi trạng thái Online/Offline.

#### Server → Client (On Methods)

| Method | Payload | Mô tả |
|--------|---------|-------|
| `UserOnline` | `{ userId: string }` | Một user vừa online |
| `UserOffline` | `{ userId: string, lastSeenAt: string }` | Một user vừa offline |
| `OnlineUsersSnapshot` | `{ userIds: string[] }` | Danh sách user online hiện tại (khi kết nối lần đầu) |

> **Cơ chế hoạt động:** Server tự động detect khi connection connect/disconnect. Khi user kết nối, server broadcast `UserOnline`; khi disconnect, server cập nhật `last_seen_at` và broadcast `UserOffline`.

---

### 4.12.4. Luồng Reconnection

```
Client mất kết nối
        │
        ▼
withAutomaticReconnect() → Thử kết nối lại (0s, 2s, 10s, 30s)
        │
        ▼
Kết nối thành công
        │
        ├── Re-join tất cả rooms đã tham gia
        ├── Fetch tin nhắn bị bỏ lỡ qua REST API
        └── Cập nhật trạng thái UI
```

---

## 4.13. Tổng hợp Endpoints

### REST API Summary

| Method | Endpoint | Auth | Mô tả |
|--------|----------|------|-------|
| `POST` | `/auth/register` | 🌐 | Đăng ký |
| `GET` | `/auth/verify-email` | 🌐 | Xác minh email |
| `POST` | `/auth/login` | 🌐 | Đăng nhập |
| `POST` | `/auth/refresh` | 🌐 | Làm mới token |
| `POST` | `/auth/logout` | 🔑 | Đăng xuất |
| `GET` | `/auth/google` | 🌐 | OAuth Google |
| `GET` | `/auth/github` | 🌐 | OAuth GitHub |
| `POST` | `/auth/forgot-password` | 🌐 | Quên mật khẩu |
| `POST` | `/auth/reset-password` | 🌐 | Đặt lại mật khẩu |
| `GET` | `/users/{username}` | 🌐 | Profile công khai |
| `GET` | `/users/me` | 🔑 | Profile bản thân |
| `PATCH` | `/users/me` | 🔑 | Cập nhật profile |
| `PUT` | `/users/me/password` | 🔑 | Đổi mật khẩu |
| `GET` | `/users/me/sessions` | 🔑 | Danh sách sessions |
| `DELETE` | `/users/me/sessions/{id}` | 🔑 | Thu hồi session |
| `GET` | `/users/me/bookmarks` | 🔑 | Bài đã bookmark |
| `GET` | `/users/search` | 🔑 | Tìm user |
| `GET` | `/categories` | 🌐 | Danh sách categories |
| `GET` | `/categories/{slug}` | 🌐 | Chi tiết category |
| `POST` | `/categories` | 👑 | Tạo category |
| `PUT` | `/categories/{id}` | 👑 | Sửa category |
| `DELETE` | `/categories/{id}` | 👑 | Xóa category |
| `PATCH` | `/categories/reorder` | 👑 | Sắp xếp category |
| `GET` | `/tags` | 🌐 | Danh sách tags |
| `GET` | `/posts` | 🌐 | Danh sách bài viết |
| `GET` | `/posts/{slug}` | 🌐 | Chi tiết bài viết |
| `POST` | `/posts` | 🔑 | Tạo bài viết |
| `PUT` | `/posts/{id}` | 🔑 | Sửa bài viết |
| `DELETE` | `/posts/{id}` | 🔑 | Xóa bài viết |
| `POST` | `/posts/{id}/vote` | 🔑 | Vote bài viết |
| `POST` | `/posts/{id}/bookmark` | 🔑 | Bookmark |
| `POST` | `/posts/{id}/report` | 🔑 | Báo cáo |
| `GET` | `/posts/{id}/history` | 🔑 | Lịch sử sửa |
| `GET` | `/posts/{postId}/comments` | 🌐 | Danh sách comment |
| `GET` | `/comments/{id}/replies` | 🌐 | Replies của comment |
| `POST` | `/posts/{postId}/comments` | 🔑 | Tạo comment |
| `PUT` | `/comments/{id}` | 🔑 | Sửa comment |
| `DELETE` | `/comments/{id}` | 🔑 | Xóa comment |
| `POST` | `/comments/{id}/vote` | 🔑 | Vote comment |
| `POST` | `/comments/{id}/report` | 🔑 | Báo cáo comment |
| `GET` | `/chat/rooms` | 🔑 | Danh sách rooms |
| `POST` | `/chat/rooms` | 🔑 | Tạo group room |
| `POST` | `/chat/rooms/direct` | 🔑 | Tạo/lấy DM room |
| `GET` | `/chat/rooms/{id}` | 🔑 | Chi tiết room |
| `POST` | `/chat/rooms/{id}/members` | 🔑 | Thêm thành viên |
| `DELETE` | `/chat/rooms/{id}/members/me` | 🔑 | Rời room |
| `GET` | `/chat/rooms/{id}/messages` | 🔑 | Lịch sử chat |
| `POST` | `/chat/rooms/{id}/read` | 🔑 | Đánh dấu đã đọc |
| `GET` | `/notifications` | 🔑 | Danh sách thông báo |
| `GET` | `/notifications/unread-count` | 🔑 | Số chưa đọc |
| `PATCH` | `/notifications/read` | 🔑 | Đánh dấu đã đọc |
| `DELETE` | `/notifications/{id}` | 🔑 | Xóa thông báo |
| `POST` | `/uploads` | 🔑 | Upload file |
| `PATCH` | `/moderator/posts/{id}/pin` | 🛡️ | Ghim bài |
| `PATCH` | `/moderator/posts/{id}/lock` | 🛡️ | Khóa bài |
| `PATCH` | `/moderator/posts/{id}/move` | 🛡️ | Chuyển danh mục |
| `PATCH` | `/moderator/posts/{id}/tags` | 🛡️ | Cập nhật tags |
| `GET` | `/moderator/reports` | 🛡️ | Danh sách báo cáo |
| `PATCH` | `/moderator/reports/{id}` | 🛡️ | Xử lý báo cáo |
| `GET` | `/admin/dashboard` | 👑 | Dashboard metrics |
| `GET` | `/admin/users` | 👑 | Danh sách users |
| `GET` | `/admin/users/{id}` | 👑 | Chi tiết user |
| `PATCH` | `/admin/users/{id}/role` | 👑 | Phân quyền |
| `PATCH` | `/admin/users/{id}/ban` | 👑 | Ban/Unban |
| `GET/POST/PUT/DELETE` | `/admin/tags` | 👑 | Quản lý tags |
| `POST` | `/admin/tags/merge` | 👑 | Hợp nhất tags |
| `GET` | `/admin/rate-limit/logs` | 👑 | Rate limit logs |

### SignalR Hubs Summary

| Hub | Endpoint | Auth |
|-----|----------|------|
| ChatHub | `/hubs/chat` | 🔑 |
| NotificationHub | `/hubs/notifications` | 🔑 |
| PresenceHub | `/hubs/presence` | 🔑 |

---

*Tài liệu này là một phần của bộ Detailed Specification cho dự án **Forum-Flow**.*  
*Chương tiếp theo: **Chương V — Đặc tả Bảo mật (Security Specification)***
