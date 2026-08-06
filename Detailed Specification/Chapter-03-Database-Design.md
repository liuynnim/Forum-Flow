# CHƯƠNG III: THIẾT KẾ CƠ SỞ DỮ LIỆU
## (DATABASE DESIGN & ERD)

> **Dự án:** Forum-Flow — Forum / Community Platform kết hợp Chat trực tiếp  
> **Phiên bản tài liệu:** 1.0.0  
> **Ngày tạo:** 2026-08-02  
> **Database:** PostgreSQL 16  
> **Trạng thái:** Draft

---

## Mục lục

- [3.1. Nguyên tắc Thiết kế](#31-nguyên-tắc-thiết-kế)
- [3.2. Sơ đồ ERD Tổng quan](#32-sơ-đồ-erd-tổng-quan)
- [3.3. Domain: Authentication & Users](#33-domain-authentication--users)
- [3.4. Domain: Forum](#34-domain-forum)
- [3.5. Domain: Real-time Chat](#35-domain-real-time-chat)
- [3.6. Domain: Notifications](#36-domain-notifications)
- [3.7. Domain: Moderation](#37-domain-moderation)
- [3.8. Indexing Strategy](#38-indexing-strategy)
- [3.9. Soft Delete Strategy](#39-soft-delete-strategy)
- [3.10. Database Naming Conventions](#310-database-naming-conventions)
- [3.11. Tổng quan Bảng & Quan hệ](#311-tổng-quan-bảng--quan-hệ)

---

## 3.1. Nguyên tắc Thiết kế

| Nguyên tắc | Quyết định | Chi tiết |
|-----------|-----------|---------|
| **Primary Key** | `UUID v4` | Tránh sequential ID bị đoán, an toàn hơn cho distributed systems |
| **Timestamp** | `TIMESTAMPTZ` | Lưu kèm timezone, tránh lỗi múi giờ |
| **Soft Delete** | `deleted_at TIMESTAMPTZ NULL` | Không xóa vật lý, cho phép khôi phục và audit |
| **Denormalization** | Có chọn lọc | `vote_score`, `comment_count` denormalize để tăng read performance |
| **Enum** | PostgreSQL native ENUM | Type-safe, hiệu quả lưu trữ |
| **Text Search** | PostgreSQL `tsvector` | Full-text search cho Posts mà không cần Elasticsearch |
| **JSON** | `JSONB` | Lưu dữ liệu linh hoạt (notification data, metadata) |
| **Naming** | `snake_case` | Chuẩn PostgreSQL convention |
| **Normalization** | 3NF | Chuẩn hóa đủ mức, không over-normalize gây JOIN chậm |

---

## 3.2. Sơ đồ ERD Tổng quan

```
╔══════════════════════════════════════════════════════════════════════════════╗
║                         AUTHENTICATION DOMAIN                               ║
║                                                                              ║
║  ┌─────────────────┐   ┌─────────────────┐   ┌──────────────────────┐       ║
║  │     users       │   │ oauth_providers  │   │  password_reset      │       ║
║  │─────────────────│   │─────────────────│   │  _tokens             │       ║
║  │ id (PK)         │◄──│ user_id (FK)    │   │──────────────────────│       ║
║  │ username        │   │ provider        │   │ id (PK)              │       ║
║  │ email           │   │ provider_id     │   │ user_id (FK) ────────►│       ║
║  │ password_hash   │   └─────────────────┘   │ token_hash           │       ║
║  │ role            │                         │ expires_at           │       ║
║  │ is_banned       │   ┌─────────────────┐   └──────────────────────┘       ║
║  │ ...             │   │ refresh_tokens  │                                   ║
║  └────────┬────────┘   │─────────────────│                                   ║
║           │            │ user_id (FK) ───►│                                   ║
║           │            └─────────────────┘                                   ║
╚═══════════╪════════════════════════════════════════════════════════════════╝  ║
            │                                                                   ║
╔═══════════╪════════════════════════════════════════════════════════════════╗  ║
║           │              FORUM DOMAIN                                      ║  ║
║           │                                                                ║  ║
║  ┌────────▼────────┐   ┌─────────────────┐   ┌──────────────────┐         ║  ║
║  │   categories    │   │   moderator_    │   │      tags        │         ║  ║
║  │─────────────────│   │   categories   │   │──────────────────│         ║  ║
║  │ id (PK)         │◄──│ category_id(FK)│   │ id (PK)          │         ║  ║
║  │ name, slug      │   │ user_id (FK)   │   │ name, slug       │         ║  ║
║  │ display_order   │   └─────────────────┘   └────────┬─────────┘         ║  ║
║  └────────┬────────┘                                  │                   ║  ║
║           │                                  ┌────────▼─────────┐         ║  ║
║  ┌────────▼────────────────────────┐         │    post_tags     │         ║  ║
║  │            posts                │         │──────────────────│         ║  ║
║  │─────────────────────────────────│◄────────│ post_id (FK)     │         ║  ║
║  │ id (PK)                         │         │ tag_id (FK)      │         ║  ║
║  │ title, slug, content            │         └──────────────────┘         ║  ║
║  │ author_id (FK) ─────────────────►│ users                               ║  ║
║  │ category_id (FK)                │                                       ║  ║
║  │ status, is_pinned               │                                       ║  ║
║  │ vote_score, comment_count       │                                       ║  ║
║  └─────┬──────────┬────────────────┘                                       ║  ║
║        │          │                                                         ║  ║
║  ┌─────▼──┐  ┌───▼────────┐  ┌──────────────┐  ┌──────────────────────┐  ║  ║
║  │post_   │  │ bookmarks  │  │   comments   │  │  post_edit_history   │  ║  ║
║  │votes   │  │────────────│  │──────────────│  │──────────────────────│  ║  ║
║  │────────│  │ user_id(FK)│  │ id (PK)      │  │ post_id (FK)         │  ║  ║
║  │post_id │  │ post_id(FK)│  │ post_id (FK) │  │ editor_id (FK)       │  ║  ║
║  │user_id │  └────────────┘  │ author_id(FK)│  │ previous_content     │  ║  ║
║  │vote_   │                  │ parent_id(FK)│  └──────────────────────┘  ║  ║
║  │type    │                  │ vote_score   │                             ║  ║
║  └────────┘                  └──────┬───────┘                             ║  ║
║                                     │                                      ║  ║
║                              ┌──────▼───────┐                             ║  ║
║                              │comment_votes │                             ║  ║
║                              │──────────────│                             ║  ║
║                              │ comment_id   │                             ║  ║
║                              │ user_id      │                             ║  ║
║                              └──────────────┘                             ║  ║
╚════════════════════════════════════════════════════════════════════════════╝  ║
                                                                                ║
╔════════════════════════════════════════════════════════════════════════════╗  ║
║                           CHAT DOMAIN                                      ║  ║
║                                                                            ║  ║
║  ┌──────────────────┐   ┌──────────────────────┐   ┌───────────────────┐  ║  ║
║  │   chat_rooms     │   │  chat_room_members   │   │     messages      │  ║  ║
║  │──────────────────│   │──────────────────────│   │───────────────────│  ║  ║
║  │ id (PK)          │◄──│ room_id (FK)         │   │ id (PK)           │  ║  ║
║  │ name             │   │ user_id (FK) ─────────►  │ room_id (FK) ─────►│  ║  ║
║  │ type (DM/GROUP)  │   │ role                 │   │ sender_id (FK)    │  ║  ║
║  │ created_by (FK)  │   │ last_read_at         │   │ content, type     │  ║  ║
║  │ last_message_at  │   └──────────────────────┘   │ file_url          │  ║  ║
║  └──────────────────┘                              └───────────────────┘  ║  ║
╚════════════════════════════════════════════════════════════════════════════╝  ║
                                                                                ║
╔════════════════════════════════════════════════════════════════════════════╗  ║
║              NOTIFICATION & MODERATION DOMAIN                              ║  ║
║                                                                            ║  ║
║  ┌──────────────────────┐         ┌──────────────────────────┐            ║  ║
║  │    notifications     │         │         reports          │            ║  ║
║  │──────────────────────│         │──────────────────────────│            ║  ║
║  │ id (PK)              │         │ id (PK)                  │            ║  ║
║  │ recipient_id (FK)    │         │ reporter_id (FK)         │            ║  ║
║  │ sender_id (FK)       │         │ content_type             │            ║  ║
║  │ type                 │         │ content_id               │            ║  ║
║  │ source_type/id       │         │ reason, status           │            ║  ║
║  │ data (JSONB)         │         │ resolved_by (FK)         │            ║  ║
║  │ is_read              │         └──────────────────────────┘            ║  ║
║  └──────────────────────┘                                                  ║  ║
╚════════════════════════════════════════════════════════════════════════════╝  ║
```

---

## 3.3. Domain: Authentication & Users

### 3.3.1. Bảng `users`

Bảng trung tâm của hệ thống — lưu thông tin tất cả người dùng.

```sql
CREATE TYPE user_role AS ENUM ('MEMBER', 'MODERATOR', 'ADMIN');

CREATE TABLE users (
    id                  UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    username            VARCHAR(50)     NOT NULL,
    email               VARCHAR(255)    NOT NULL,
    password_hash       VARCHAR(500)    NULL,           -- NULL nếu chỉ dùng OAuth
    display_name        VARCHAR(100)    NOT NULL,
    avatar_url          VARCHAR(500)    NULL,
    bio                 TEXT            NULL,
    website_url         VARCHAR(255)    NULL,
    github_url          VARCHAR(255)    NULL,
    role                user_role       NOT NULL DEFAULT 'MEMBER',
    is_email_verified   BOOLEAN         NOT NULL DEFAULT FALSE,
    is_banned           BOOLEAN         NOT NULL DEFAULT FALSE,
    ban_reason          TEXT            NULL,
    ban_until           TIMESTAMPTZ     NULL,           -- NULL = vĩnh viễn
    last_seen_at        TIMESTAMPTZ     NULL,
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    deleted_at          TIMESTAMPTZ     NULL            -- Soft delete
);

-- Constraints
ALTER TABLE users ADD CONSTRAINT uq_users_username UNIQUE (username);
ALTER TABLE users ADD CONSTRAINT uq_users_email    UNIQUE (email);
ALTER TABLE users ADD CONSTRAINT chk_users_username_length
    CHECK (LENGTH(username) >= 3);
ALTER TABLE users ADD CONSTRAINT chk_users_ban_logic
    CHECK (is_banned = FALSE OR ban_reason IS NOT NULL);
```

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `id` | UUID | PK | Auto-generated UUID v4 |
| `username` | VARCHAR(50) | UNIQUE, NOT NULL, ≥3 ký tự | Định danh duy nhất, dùng cho @mention |
| `email` | VARCHAR(255) | UNIQUE, NOT NULL | Email đăng nhập |
| `password_hash` | VARCHAR(500) | NULL allowed | NULL nếu user chỉ dùng OAuth2 |
| `display_name` | VARCHAR(100) | NOT NULL | Tên hiển thị (có thể trùng) |
| `avatar_url` | VARCHAR(500) | NULL | URL ảnh đại diện từ Cloudinary |
| `role` | ENUM | NOT NULL, DEFAULT 'MEMBER' | Phân quyền RBAC |
| `is_banned` | BOOLEAN | DEFAULT FALSE | Trạng thái bị khóa |
| `ban_until` | TIMESTAMPTZ | NULL | NULL = ban vĩnh viễn |
| `last_seen_at` | TIMESTAMPTZ | NULL | Phục vụ Online/Offline status |
| `deleted_at` | TIMESTAMPTZ | NULL | Soft delete marker |

---

### 3.3.2. Bảng `oauth_providers`

Lưu thông tin tài khoản OAuth2 liên kết với user.

```sql
CREATE TABLE oauth_providers (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    provider        VARCHAR(50) NOT NULL,   -- 'google' | 'github'
    provider_id     VARCHAR(255) NOT NULL,  -- ID từ provider
    provider_email  VARCHAR(255) NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE oauth_providers
    ADD CONSTRAINT uq_oauth_provider UNIQUE (provider, provider_id);
```

---

### 3.3.3. Bảng `refresh_tokens`

Lưu Refresh Token để hỗ trợ revocation (Redis là cache nhanh, PostgreSQL là nguồn gốc).

```sql
CREATE TABLE refresh_tokens (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash  VARCHAR(500) NOT NULL,      -- Hash của Refresh Token (bcrypt/SHA256)
    expires_at  TIMESTAMPTZ NOT NULL,
    is_revoked  BOOLEAN     NOT NULL DEFAULT FALSE,
    device_info VARCHAR(255) NULL,          -- User-Agent info
    ip_address  VARCHAR(45) NULL,           -- IPv4 / IPv6
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

---

### 3.3.4. Bảng `password_reset_tokens`

```sql
CREATE TABLE password_reset_tokens (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash  VARCHAR(500) NOT NULL,
    expires_at  TIMESTAMPTZ NOT NULL,
    is_used     BOOLEAN     NOT NULL DEFAULT FALSE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

---

## 3.4. Domain: Forum

### 3.4.1. Bảng `categories`

```sql
CREATE TABLE categories (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100) NOT NULL,
    slug            VARCHAR(100) NOT NULL,
    description     TEXT        NULL,
    icon            VARCHAR(50) NULL,       -- Icon name (e.g., 'code', 'chat')
    color           VARCHAR(7)  NULL,       -- Hex color '#3B82F6'
    display_order   SMALLINT    NOT NULL DEFAULT 0,
    is_active       BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE categories ADD CONSTRAINT uq_categories_slug UNIQUE (slug);
ALTER TABLE categories ADD CONSTRAINT chk_categories_color
    CHECK (color ~ '^#[0-9A-Fa-f]{6}$');
```

---

### 3.4.2. Bảng `moderator_categories`

Quan hệ N-N: một Moderator quản lý nhiều Category, một Category có nhiều Moderator.

```sql
CREATE TABLE moderator_categories (
    user_id         UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    category_id     UUID        NOT NULL REFERENCES categories(id) ON DELETE CASCADE,
    assigned_by     UUID        NOT NULL REFERENCES users(id),  -- Admin giao
    assigned_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (user_id, category_id)
);
```

---

### 3.4.3. Bảng `tags`

```sql
CREATE TABLE tags (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(50) NOT NULL,
    slug        VARCHAR(50) NOT NULL,
    description TEXT        NULL,
    post_count  INT         NOT NULL DEFAULT 0,  -- Denormalized counter
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE tags ADD CONSTRAINT uq_tags_name UNIQUE (name);
ALTER TABLE tags ADD CONSTRAINT uq_tags_slug UNIQUE (slug);
```

---

### 3.4.4. Bảng `posts`

Bảng trung tâm của Forum domain.

```sql
CREATE TYPE post_status AS ENUM ('DRAFT', 'PUBLISHED', 'LOCKED', 'DELETED');

CREATE TABLE posts (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    title           VARCHAR(500)    NOT NULL,
    slug            VARCHAR(600)    NOT NULL,
    content         TEXT            NOT NULL,   -- Markdown source
    content_html    TEXT            NULL,       -- Rendered HTML (cached)
    author_id       UUID            NOT NULL REFERENCES users(id),
    category_id     UUID            NOT NULL REFERENCES categories(id),
    status          post_status     NOT NULL DEFAULT 'PUBLISHED',
    is_pinned       BOOLEAN         NOT NULL DEFAULT FALSE,
    view_count      INT             NOT NULL DEFAULT 0,
    vote_score      INT             NOT NULL DEFAULT 0, -- Denormalized: SUM(vote_type)
    comment_count   INT             NOT NULL DEFAULT 0, -- Denormalized counter
    search_vector   TSVECTOR        NULL,               -- Full-text search index
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    deleted_at      TIMESTAMPTZ     NULL
);

ALTER TABLE posts ADD CONSTRAINT uq_posts_slug UNIQUE (slug);
ALTER TABLE posts ADD CONSTRAINT chk_posts_title_length
    CHECK (LENGTH(title) >= 10);

-- Full-text search: kết hợp title (weight A) và content (weight B)
CREATE INDEX idx_posts_search ON posts USING GIN (search_vector);

-- Trigger tự động cập nhật search_vector khi INSERT/UPDATE
CREATE OR REPLACE FUNCTION update_post_search_vector()
RETURNS TRIGGER AS $$
BEGIN
    NEW.search_vector :=
        setweight(to_tsvector('english', COALESCE(NEW.title, '')), 'A') ||
        setweight(to_tsvector('english', COALESCE(NEW.content, '')), 'B');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_posts_search_vector
    BEFORE INSERT OR UPDATE ON posts
    FOR EACH ROW EXECUTE FUNCTION update_post_search_vector();
```

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `slug` | VARCHAR(600) | UNIQUE — URL-friendly: `tieu-de-bai-viet-{nanoid}` |
| `content` | TEXT | Markdown source lưu gốc |
| `content_html` | TEXT | HTML đã render, cache để hiển thị nhanh |
| `vote_score` | INT | Denormalized `SUM(vote_type)` từ `post_votes` |
| `comment_count` | INT | Denormalized counter, update bằng trigger |
| `search_vector` | TSVECTOR | Full-text search PostgreSQL native |
| `status` | ENUM | `PUBLISHED` \| `LOCKED` \| `DRAFT` \| `DELETED` |

---

### 3.4.5. Bảng `post_tags`

```sql
CREATE TABLE post_tags (
    post_id     UUID    NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    tag_id      UUID    NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
    PRIMARY KEY (post_id, tag_id)
);
```

---

### 3.4.6. Bảng `post_votes`

```sql
CREATE TABLE post_votes (
    post_id     UUID        NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    user_id     UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    vote_type   SMALLINT    NOT NULL,       -- 1 = upvote, -1 = downvote
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (post_id, user_id)
);

ALTER TABLE post_votes ADD CONSTRAINT chk_post_votes_type
    CHECK (vote_type IN (1, -1));
```

> **Trigger cập nhật `posts.vote_score`** khi INSERT/UPDATE/DELETE vào `post_votes`.

---

### 3.4.7. Bảng `bookmarks`

```sql
CREATE TABLE bookmarks (
    user_id     UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    post_id     UUID        NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (user_id, post_id)
);
```

---

### 3.4.8. Bảng `comments`

Nested Comments sử dụng **Adjacency List** (parent_comment_id tự tham chiếu).

```sql
CREATE TABLE comments (
    id                  UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id             UUID        NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    author_id           UUID        NOT NULL REFERENCES users(id),
    parent_comment_id   UUID        NULL REFERENCES comments(id) ON DELETE CASCADE,
    content             TEXT        NOT NULL,   -- Markdown
    vote_score          INT         NOT NULL DEFAULT 0,
    depth               SMALLINT    NOT NULL DEFAULT 0, -- 0=root, 1=reply, 2=reply-of-reply
    reply_count         INT         NOT NULL DEFAULT 0, -- Denormalized
    is_deleted          BOOLEAN     NOT NULL DEFAULT FALSE,
    deleted_at          TIMESTAMPTZ NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE comments ADD CONSTRAINT chk_comments_depth
    CHECK (depth <= 5);  -- Giới hạn tối đa 5 cấp lồng nhau
ALTER TABLE comments ADD CONSTRAINT chk_comments_content
    CHECK (LENGTH(TRIM(content)) > 0);
```

> **Tại sao Adjacency List?** Phù hợp với trường hợp đọc comment theo từng post, load from root level, và depth giới hạn ≤ 5 cấp. Nếu cần query toàn bộ cây, dùng Recursive CTE (`WITH RECURSIVE`).

---

### 3.4.9. Bảng `comment_votes`

```sql
CREATE TABLE comment_votes (
    comment_id  UUID        NOT NULL REFERENCES comments(id) ON DELETE CASCADE,
    user_id     UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    vote_type   SMALLINT    NOT NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (comment_id, user_id)
);

ALTER TABLE comment_votes ADD CONSTRAINT chk_comment_votes_type
    CHECK (vote_type IN (1, -1));
```

---

### 3.4.10. Bảng `post_edit_history`

```sql
CREATE TABLE post_edit_history (
    id                  UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id             UUID        NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    editor_id           UUID        NOT NULL REFERENCES users(id),
    previous_title      VARCHAR(500) NOT NULL,
    previous_content    TEXT        NOT NULL,
    edit_reason         VARCHAR(255) NULL,
    edited_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

---

## 3.5. Domain: Real-time Chat

### 3.5.1. Bảng `chat_rooms`

```sql
CREATE TYPE chat_room_type AS ENUM ('DIRECT', 'GROUP');

CREATE TABLE chat_rooms (
    id                  UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    name                VARCHAR(100)    NULL,       -- NULL cho DIRECT (1-1)
    type                chat_room_type  NOT NULL DEFAULT 'GROUP',
    created_by          UUID            NOT NULL REFERENCES users(id),
    avatar_url          VARCHAR(500)    NULL,
    last_message_at     TIMESTAMPTZ     NULL,
    last_message_preview VARCHAR(100)   NULL,       -- Preview tin nhắn cuối
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- Đảm bảo DIRECT chat không có tên
ALTER TABLE chat_rooms ADD CONSTRAINT chk_room_direct_no_name
    CHECK (type != 'DIRECT' OR name IS NULL);
```

---

### 3.5.2. Bảng `chat_room_members`

```sql
CREATE TYPE room_member_role AS ENUM ('MEMBER', 'ADMIN');

CREATE TABLE chat_room_members (
    room_id         UUID                NOT NULL REFERENCES chat_rooms(id) ON DELETE CASCADE,
    user_id         UUID                NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role            room_member_role    NOT NULL DEFAULT 'MEMBER',
    joined_at       TIMESTAMPTZ         NOT NULL DEFAULT NOW(),
    last_read_at    TIMESTAMPTZ         NULL,       -- Dùng tính unread count
    is_muted        BOOLEAN             NOT NULL DEFAULT FALSE,
    PRIMARY KEY (room_id, user_id)
);
```

---

### 3.5.3. Bảng `messages`

```sql
CREATE TYPE message_type AS ENUM ('TEXT', 'IMAGE', 'FILE');

CREATE TABLE messages (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    room_id         UUID            NOT NULL REFERENCES chat_rooms(id) ON DELETE CASCADE,
    sender_id       UUID            NOT NULL REFERENCES users(id),
    content         TEXT            NULL,       -- NULL nếu là file-only message
    type            message_type    NOT NULL DEFAULT 'TEXT',
    file_url        VARCHAR(500)    NULL,
    file_name       VARCHAR(255)    NULL,
    file_size       BIGINT          NULL,       -- bytes
    file_mime_type  VARCHAR(100)    NULL,
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at      TIMESTAMPTZ     NULL,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

ALTER TABLE messages ADD CONSTRAINT chk_messages_content
    CHECK (content IS NOT NULL OR file_url IS NOT NULL);
```

> **Tính unread count:** `SELECT COUNT(*) FROM messages WHERE room_id = ? AND created_at > (SELECT last_read_at FROM chat_room_members WHERE room_id = ? AND user_id = ?)`

---

## 3.6. Domain: Notifications

### 3.6.1. Bảng `notifications`

```sql
CREATE TYPE notification_type AS ENUM (
    'POST_UPVOTED',
    'POST_COMMENTED',
    'COMMENT_REPLIED',
    'USER_MENTIONED',
    'NEW_MESSAGE',
    'SYSTEM'
);

CREATE TABLE notifications (
    id              UUID                PRIMARY KEY DEFAULT gen_random_uuid(),
    recipient_id    UUID                NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    sender_id       UUID                NULL REFERENCES users(id) ON DELETE SET NULL,
    type            notification_type   NOT NULL,
    source_type     VARCHAR(20)         NOT NULL,   -- 'post' | 'comment' | 'message'
    source_id       UUID                NOT NULL,   -- ID của post/comment/message
    data            JSONB               NULL,       -- Context data linh hoạt
    is_read         BOOLEAN             NOT NULL DEFAULT FALSE,
    read_at         TIMESTAMPTZ         NULL,
    created_at      TIMESTAMPTZ         NOT NULL DEFAULT NOW()
);
```

**Ví dụ `data` JSONB:**
```json
// POST_UPVOTED
{ "post_title": "Hướng dẫn dùng SignalR", "post_slug": "huong-dan-dung-signalr" }

// USER_MENTIONED
{ "post_title": "Review macbook M4", "comment_preview": "@johndoe bạn nghĩ sao về..." }

// NEW_MESSAGE
{ "room_name": "Team Frontend", "room_type": "GROUP", "message_preview": "Hey có ai rảnh không..." }
```

---

## 3.7. Domain: Moderation

### 3.7.1. Bảng `reports`

```sql
CREATE TYPE report_status AS ENUM ('PENDING', 'APPROVED', 'REJECTED');

CREATE TABLE reports (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    reporter_id     UUID            NOT NULL REFERENCES users(id),
    content_type    VARCHAR(20)     NOT NULL,   -- 'post' | 'comment'
    content_id      UUID            NOT NULL,
    reason          VARCHAR(100)    NOT NULL,   -- 'spam' | 'offensive' | 'misinformation' ...
    description     TEXT            NULL,
    status          report_status   NOT NULL DEFAULT 'PENDING',
    resolved_by     UUID            NULL REFERENCES users(id),  -- Moderator/Admin
    resolved_at     TIMESTAMPTZ     NULL,
    resolution_note TEXT            NULL,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- Mỗi user chỉ báo cáo 1 lần cho mỗi nội dung
ALTER TABLE reports
    ADD CONSTRAINT uq_reports_unique
    UNIQUE (reporter_id, content_type, content_id);
```

---

## 3.8. Indexing Strategy

### Posts — Indexes

```sql
-- Lọc theo Category + sắp xếp (phổ biến nhất)
CREATE INDEX idx_posts_category_created   ON posts (category_id, created_at DESC)
    WHERE deleted_at IS NULL AND status = 'PUBLISHED';

-- Hot posts (sắp xếp theo điểm vote)
CREATE INDEX idx_posts_category_vote      ON posts (category_id, vote_score DESC)
    WHERE deleted_at IS NULL AND status = 'PUBLISHED';

-- Bài viết của một user
CREATE INDEX idx_posts_author             ON posts (author_id, created_at DESC)
    WHERE deleted_at IS NULL;

-- Bài ghim lên đầu
CREATE INDEX idx_posts_pinned             ON posts (category_id, is_pinned DESC, created_at DESC)
    WHERE is_pinned = TRUE AND status = 'PUBLISHED';
```

### Comments — Indexes

```sql
-- Lấy tất cả comment root của một post
CREATE INDEX idx_comments_post_root   ON comments (post_id, created_at ASC)
    WHERE parent_comment_id IS NULL AND is_deleted = FALSE;

-- Lấy replies của một comment
CREATE INDEX idx_comments_parent      ON comments (parent_comment_id, created_at ASC)
    WHERE is_deleted = FALSE;

-- Bình luận của một user
CREATE INDEX idx_comments_author      ON comments (author_id, created_at DESC);
```

### Messages — Indexes

```sql
-- Lịch sử chat theo room (phân trang ngược từ mới nhất)
CREATE INDEX idx_messages_room_time   ON messages (room_id, created_at DESC)
    WHERE is_deleted = FALSE;

-- Đếm tin nhắn chưa đọc
CREATE INDEX idx_messages_room_created ON messages (room_id, created_at);
```

### Notifications — Indexes

```sql
-- Inbox của user: chưa đọc trước, rồi đến mới nhất
CREATE INDEX idx_notifications_recipient ON notifications
    (recipient_id, is_read ASC, created_at DESC);
```

### Users — Indexes

```sql
-- Tìm kiếm user theo email (login)
CREATE INDEX idx_users_email      ON users (email) WHERE deleted_at IS NULL;

-- Tìm kiếm user theo username (@mention, search)
CREATE INDEX idx_users_username   ON users (username) WHERE deleted_at IS NULL;

-- Lọc user theo role (Admin dashboard)
CREATE INDEX idx_users_role       ON users (role, created_at DESC);
```

### Reports — Indexes

```sql
CREATE INDEX idx_reports_status_type ON reports (status, content_type, created_at DESC);
```

---

## 3.9. Soft Delete Strategy

Các bảng áp dụng Soft Delete (thêm cột `deleted_at`):

| Bảng | Lý do |
|------|-------|
| `users` | Cần giữ lại lịch sử bài viết / bình luận; cho phép khôi phục tài khoản |
| `posts` | Moderator có thể khôi phục bài bị xóa nhầm |
| `comments` | Khi xóa comment cha, giữ lại cấu trúc cây, chỉ ẩn nội dung |
| `messages` | Tin nhắn đã xóa vẫn hiển thị placeholder "Message deleted" |

**Quy ước:**
- `deleted_at IS NULL` → bản ghi còn tồn tại (active)
- `deleted_at IS NOT NULL` → đã bị xóa mềm
- Tất cả query bình thường thêm filter `WHERE deleted_at IS NULL`
- Partial indexes đã kết hợp điều kiện này để tối ưu hiệu năng

---

## 3.10. Database Naming Conventions

| Quy ước | Ví dụ |
|---------|-------|
| Tên bảng | `snake_case`, số nhiều: `users`, `posts`, `chat_rooms` |
| Tên cột | `snake_case`: `created_at`, `vote_score`, `is_deleted` |
| Primary Key | `id UUID` |
| Foreign Key | `{table_singular}_id`: `user_id`, `post_id`, `room_id` |
| Index | `idx_{table}_{columns}`: `idx_posts_category_created` |
| Unique Constraint | `uq_{table}_{column}`: `uq_users_email` |
| Check Constraint | `chk_{table}_{rule}`: `chk_posts_title_length` |
| ENUM type | `{entity}_{field}`: `user_role`, `post_status`, `message_type` |
| Trigger | `trg_{table}_{action}`: `trg_posts_search_vector` |

---

## 3.11. Tổng quan Bảng & Quan hệ

### Danh sách tất cả bảng

| Bảng | Domain | Rows ước tính | Ghi chú |
|------|--------|--------------|---------|
| `users` | Auth | Vừa | Bảng trung tâm |
| `oauth_providers` | Auth | Vừa | 1-N với users |
| `refresh_tokens` | Auth | Lớn | Rotate thường xuyên |
| `password_reset_tokens` | Auth | Nhỏ | Cleanup định kỳ |
| `categories` | Forum | Rất nhỏ | < 100 records |
| `moderator_categories` | Forum | Nhỏ | Junction table |
| `tags` | Forum | Nhỏ | < 1000 records |
| `posts` | Forum | **Lớn** | Bảng đọc/ghi nhiều nhất |
| `post_tags` | Forum | Lớn | Junction table |
| `post_votes` | Forum | Rất lớn | Tối ưu với composite PK |
| `bookmarks` | Forum | Vừa | Composite PK |
| `comments` | Forum | **Rất lớn** | Self-referencing |
| `comment_votes` | Forum | Rất lớn | Composite PK |
| `post_edit_history` | Forum | Vừa | Append-only |
| `chat_rooms` | Chat | Vừa | |
| `chat_room_members` | Chat | Vừa | Junction |
| `messages` | Chat | **Rất lớn** | Partition theo tháng (tương lai) |
| `notifications` | Notif | **Rất lớn** | Archive sau 90 ngày |
| `reports` | Moderation | Vừa | |

### Sơ đồ Quan hệ Đơn giản

```
users ──────────────────────────────────────────────────────────────────────┐
  │ 1                                                                        │
  ├──N── oauth_providers                                                     │
  ├──N── refresh_tokens                                                      │
  ├──N── password_reset_tokens                                               │
  │                                                                          │
  ├──N── posts (author_id) ──N── post_votes ──N── users                      │
  │         │               └── post_tags ──N── tags                         │
  │         │               └── bookmarks ──N── users                        │
  │         │               └── comments (self-ref, parent_comment_id)       │
  │         │                        └── comment_votes ──N── users           │
  │         │               └── post_edit_history                            │
  │         │                                                                │
  ├──N── moderator_categories ──N── categories ──N── posts                  │
  │                                                                          │
  ├──N── chat_room_members ──N── chat_rooms ──N── messages                  │
  │                                                                          │
  ├──N── notifications (recipient_id / sender_id)                           │
  │                                                                          │
  └──N── reports (reporter_id / resolved_by) ──────────────────────────────┘
```

---

*Tài liệu này là một phần của bộ Detailed Specification cho dự án **Forum-Flow**.*  
*Chương tiếp theo: **Chương IV — Đặc tả API (API Specification - REST & SignalR)***
