# CHƯƠNG I: XÁC ĐỊNH YÊU CẦU HỆ THỐNG
## (REQUIREMENTS SPECIFICATION)

> **Dự án:** Forum-Flow — Forum / Community Platform kết hợp Chat trực tiếp  
> **Phiên bản tài liệu:** 1.0.0  
> **Ngày tạo:** 2026-08-02  
> **Trạng thái:** Draft

---

## Mục lục

- [1.1. Các Tác nhân Hệ thống (Actors)](#11-các-tác-nhân-hệ-thống-actors)
- [1.2. Danh sách Chức năng chi tiết cho từng Tác nhân](#12-danh-sách-chức-năng-chi-tiết-cho-từng-tác-nhân-actor-functions)
  - [1.2.1. Khách (Guest)](#121-khách-guest)
  - [1.2.2. Thành viên (Member)](#122-thành-viên-member)
  - [1.2.3. Kiểm duyệt viên (Moderator)](#123-kiểm-duyệt-viên-moderator)
  - [1.2.4. Quản trị viên (Administrator)](#124-quản-trị-viên-administrator)
- [1.3. Ma trận Phân quyền (Permission Matrix)](#13-ma-trận-phân-quyền-permission-matrix)

---

## 1.1. Các Tác nhân Hệ thống (Actors)

Hệ thống áp dụng mô hình **RBAC (Role-Based Access Control)** — phân quyền dựa trên vai trò, với **4 tác nhân chính** xếp theo cấp độ quyền hạn tăng dần:

```
Guest  ⊂  Member  ⊂  Moderator  ⊂  Administrator
```

| STT | Vai trò | Ký hiệu | Mô tả |
|-----|---------|---------|-------|
| 1 | **Khách** | `GUEST` | Người dùng chưa xác thực, chỉ truy cập nội dung công khai |
| 2 | **Thành viên** | `MEMBER` | Người dùng đã đăng ký & đăng nhập thành công |
| 3 | **Kiểm duyệt viên** | `MODERATOR` | Thành viên được cấp quyền quản lý nội dung theo chuyên mục được phân công |
| 4 | **Quản trị viên** | `ADMIN` | Tác nhân có quyền cao nhất, kiểm soát toàn bộ hệ thống |

> **Nguyên tắc kế thừa:** Mỗi vai trò cấp cao hơn bao gồm toàn bộ quyền của vai trò cấp thấp hơn liền kề.

---

## 1.2. Danh sách Chức năng chi tiết cho từng Tác nhân (Actor Functions)

---

### 1.2.1. Khách (Guest)

> Tác nhân này chủ yếu tương tác với nội dung **tĩnh** hoặc **công khai**, được tối ưu hóa cho SEO.

#### 🔐 Tài khoản & Xác thực

| Mã | Chức năng | Mô tả chi tiết |
|----|-----------|---------------|
| G-AUTH-01 | **Đăng ký tài khoản mới** | Đăng ký bằng cặp Email / Password. Gửi email xác minh tài khoản sau đăng ký. |
| G-AUTH-02 | **Đăng nhập** | Hỗ trợ 2 phương thức: *(i)* JWT (Email/Password), *(ii)* OAuth2 (Google, GitHub). |
| G-AUTH-03 | **Khôi phục mật khẩu** | Yêu cầu đặt lại mật khẩu qua Email OTP hoặc Magic Link có thời hạn. |

#### 📖 Xem nội dung Forum

| Mã | Chức năng | Mô tả chi tiết |
|----|-----------|---------------|
| G-FORUM-01 | **Xem danh sách Chuyên mục** | Hiển thị toàn bộ Categories và số lượng Topics bên trong. |
| G-FORUM-02 | **Xem danh sách Bài viết** | Duyệt Topics/Posts trong từng Category. |
| G-FORUM-03 | **Tìm kiếm & Lọc bài viết** | Tìm kiếm theo từ khóa (Full-text search); lọc theo Tags; sắp xếp theo: *Mới nhất, Hot nhất, Nhiều lượt xem*. |
| G-FORUM-04 | **Xem chi tiết bài viết** | Hiển thị nội dung đầy đủ (Markdown rendered) và danh sách bình luận phân cấp (Nested Comments). |
| G-FORUM-05 | **Xem Profile công khai** | Xem thông tin cá nhân công khai (Avatar, Bio, bài đăng) của người dùng khác. |

---

### 1.2.2. Thành viên (Member)

> Bao gồm **toàn bộ chức năng của Khách**, cộng thêm các quyền tương tác Forum và Real-time Chat.

#### 👤 Quản lý Tài khoản Cá nhân

| Mã | Chức năng | Mô tả chi tiết |
|----|-----------|---------------|
| M-PROFILE-01 | **Cập nhật thông tin cá nhân** | Chỉnh sửa Avatar, Bio, các đường dẫn mạng xã hội (Social Links). |
| M-PROFILE-02 | **Đổi mật khẩu** | Xác thực mật khẩu cũ trước khi cho phép đặt mật khẩu mới. |
| M-PROFILE-03 | **Quản lý phiên đăng nhập** | Xem danh sách Active Sessions (thiết bị, địa điểm, thời gian); cho phép đăng xuất từng phiên từ xa. |

#### 📝 Quản lý Bài viết & Tương tác (Forum Core)

| Mã | Chức năng | Mô tả chi tiết |
|----|-----------|---------------|
| M-FORUM-01 | **Tạo bài viết mới** | Soạn thảo nội dung bằng Markdown Editor (hỗ trợ preview trực tiếp); upload ảnh/file đính kèm lên Cloud Storage (S3 / Cloudinary). |
| M-FORUM-02 | **Chỉnh sửa bài viết** | Chỉ cho phép chỉnh sửa bài viết **do chính mình tạo ra**; lưu lịch sử chỉnh sửa (Edit History). |
| M-FORUM-03 | **Xóa bài viết** | Chỉ cho phép xóa bài viết **do chính mình tạo ra**; hỗ trợ Soft Delete. |
| M-FORUM-04 | **Upvote / Downvote** | Bình chọn tích cực / tiêu cực cho bài viết; điểm số ảnh hưởng đến thuật toán sắp xếp Hot. |
| M-FORUM-05 | **Lưu bài viết (Bookmark)** | Đánh dấu bài viết yêu thích để truy cập nhanh từ trang Profile. |
| M-FORUM-06 | **Viết bình luận** | Bình luận trực tiếp vào bài viết; hỗ trợ Nested Comments (trả lời bình luận theo luồng phân cấp). |
| M-FORUM-07 | **Mention người dùng** | Dùng cú pháp `@username` trong bài viết và bình luận để gắn thẻ người dùng khác; người được mention sẽ nhận thông báo. |

#### 💬 Trò chuyện Trực tiếp (Real-time Chat)

| Mã | Chức năng | Mô tả chi tiết |
|----|-----------|---------------|
| M-CHAT-01 | **Chat 1-1 (Private Message)** | Tìm kiếm người dùng theo username/email; gửi và nhận tin nhắn thời gian thực qua WebSocket. |
| M-CHAT-02 | **Tạo phòng Chat nhóm (Group Chat / Room)** | Tạo phòng chat, đặt tên phòng, thêm thành viên vào nhóm. |
| M-CHAT-03 | **Tham gia / Rời phòng Chat** | Tham gia phòng chat công khai; rời phòng chat bất kỳ lúc nào. |
| M-CHAT-04 | **Trạng thái Online / Offline** | Hiển thị badge trạng thái trực tuyến của người dùng theo thời gian thực. |
| M-CHAT-05 | **Trạng thái "Đang gõ..." (Typing Indicator)** | Hiển thị thông báo `[Tên] đang gõ...` khi đối phương đang soạn tin nhắn. |
| M-CHAT-06 | **Gửi File / Hình ảnh qua Chat** | Upload và gửi file, ảnh trực tiếp trong hộp chat; xem preview ảnh inline. |
| M-CHAT-07 | **Thông báo tin nhắn chưa đọc** | Hiển thị badge số lượng tin nhắn chưa đọc trên icon và danh sách cuộc trò chuyện. |

#### 🔔 Hệ thống Thông báo (Notifications)

| Mã | Chức năng | Mô tả chi tiết |
|----|-----------|---------------|
| M-NOTIF-01 | **Thông báo thời gian thực** | Nhận thông báo dạng Popup / Toast và cập nhật số đếm trên Bell Icon ngay lập tức qua WebSocket / SSE. |
| M-NOTIF-02 | **Sự kiện kích hoạt thông báo** | Gồm 4 sự kiện: *(i)* Có người **Upvote** bài viết của mình, *(ii)* Có người **Comment** vào bài của mình, *(iii)* Được **Mention** (`@username`) trong bài viết/comment, *(iv)* Nhận **Tin nhắn Chat** mới. |
| M-NOTIF-03 | **Xem danh sách Thông báo** | Trang/Dropdown hiển thị lịch sử thông báo; đánh dấu đã đọc / xóa thông báo. |

---

### 1.2.3. Kiểm duyệt viên (Moderator)

> Bao gồm **toàn bộ chức năng của Thành viên**, bổ sung thêm công cụ quản lý nội dung cộng đồng.

> ⚠️ **Phạm vi quyền hạn:** Moderator chỉ có quyền kiểm duyệt trong các **Chuyên mục được phân công** bởi Admin.

#### 📌 Kiểm duyệt Bài viết

| Mã | Chức năng | Mô tả chi tiết |
|----|-----------|---------------|
| MOD-POST-01 | **Ghim bài viết (Pin Post)** | Ghim bài viết quan trọng lên đầu danh sách trong chuyên mục; bài ghim được hiển thị ưu tiên. |
| MOD-POST-02 | **Khóa bài viết (Lock Topic)** | Khóa một bài viết để ngăn không cho thành viên đăng thêm bình luận; hiển thị nhãn "Đã khóa". |
| MOD-POST-03 | **Gắn lại Thẻ (Re-tag)** | Chỉnh sửa thẻ (Tags) của bài viết để phân loại lại cho đúng chủ đề. |
| MOD-POST-04 | **Chuyển chuyên mục (Move Post)** | Di chuyển bài viết sang Category phù hợp hơn trong phạm vi quản lý. |
| MOD-POST-05 | **Xóa bài viết / Bình luận vi phạm** | Xóa các bài viết hoặc bình luận không tuân thủ tiêu chuẩn cộng đồng (Community Guidelines). |

#### 🚩 Xử lý Báo cáo (Reports)

| Mã | Chức năng | Mô tả chi tiết |
|----|-----------|---------------|
| MOD-REPORT-01 | **Xem danh sách Báo cáo** | Xem toàn bộ nội dung (bài viết / bình luận) đã bị thành viên Báo cáo (Report) trong chuyên mục quản lý. |
| MOD-REPORT-02 | **Xử lý Báo cáo** | Thực hiện một trong hai hành động: *(i)* **Phê duyệt** — xác nhận vi phạm, tiến hành xóa nội dung; *(ii)* **Bác bỏ** — đánh dấu báo cáo là không hợp lệ. |

---

### 1.2.4. Quản trị viên (Administrator)

> Bao gồm **toàn bộ chức năng của Moderator**, nắm quyền kiểm soát và cấu hình toàn bộ hệ thống.

#### ⚙️ Quản lý Hệ thống & Danh mục (System & Categories Management)

| Mã | Chức năng | Mô tả chi tiết |
|----|-----------|---------------|
| ADM-CAT-01 | **Tạo Chuyên mục (Category)** | Tạo mới Category với tên, slug URL, mô tả, icon và màu sắc đại diện. |
| ADM-CAT-02 | **Chỉnh sửa Chuyên mục** | Cập nhật thông tin, trạng thái hiển thị (Public / Private) của Category. |
| ADM-CAT-03 | **Xóa Chuyên mục** | Xóa Category (có cảnh báo nếu vẫn còn bài viết bên trong). |
| ADM-CAT-04 | **Sắp xếp thứ tự Chuyên mục** | Kéo-thả (Drag & Drop) để điều chỉnh thứ tự hiển thị các Category trên trang chủ Forum. |
| ADM-TAG-01 | **Quản lý Thẻ (Tags)** | Tạo mới, chỉnh sửa, hợp nhất (Merge) hoặc xóa các Tags trên toàn hệ thống. |

#### 👥 Quản lý Người dùng (User Management)

| Mã | Chức năng | Mô tả chi tiết |
|----|-----------|---------------|
| ADM-USR-01 | **Xem danh sách Người dùng** | Hiển thị toàn bộ danh sách tài khoản; hỗ trợ tìm kiếm theo Email hoặc Username; lọc theo Role, trạng thái tài khoản. |
| ADM-USR-02 | **Phân quyền Người dùng** | Thay đổi Role của tài khoản: `MEMBER` ↔ `MODERATOR`; chỉ định Moderator phụ trách Category cụ thể. |
| ADM-USR-03 | **Khóa tài khoản (Ban User)** | Ban tạm thời (kèm thời hạn) hoặc vĩnh viễn; người dùng bị Ban không thể đăng nhập; có thể gỡ Ban bất kỳ lúc nào. |

#### 📊 Thống kê & Giám sát (Dashboard & Metrics)

| Mã | Chức năng | Mô tả chi tiết |
|----|-----------|---------------|
| ADM-DASH-01 | **Tổng quan Chỉ số Hệ thống** | Widget hiển thị: Tổng số người dùng, Người dùng mới trong ngày/tuần/tháng, Số bài đăng mới/ngày, Lưu lượng tin nhắn Chat. |
| ADM-DASH-02 | **Biểu đồ Thống kê** | Biểu đồ đường/cột trực quan hóa xu hướng hoạt động theo thời gian (Daily Active Users, Posts per Day). |
| ADM-DASH-03 | **Giám sát Rate Limiting** | Xem nhật ký (Audit Log) các request bị chặn bởi Rate Limiter; phát hiện và xử lý hành vi Spam. |

---

## 1.3. Ma trận Phân quyền (Permission Matrix)

Bảng tổng hợp nhanh quyền truy cập theo từng nhóm chức năng:

| Nhóm chức năng | Guest | Member | Moderator | Admin |
|----------------|:-----:|:------:|:---------:|:-----:|
| Xem nội dung Forum công khai | ✅ | ✅ | ✅ | ✅ |
| Đăng ký / Đăng nhập | ✅ | ✅ | ✅ | ✅ |
| Tạo / Sửa / Xóa bài của mình | ❌ | ✅ | ✅ | ✅ |
| Upvote / Downvote / Bookmark | ❌ | ✅ | ✅ | ✅ |
| Bình luận & Mention | ❌ | ✅ | ✅ | ✅ |
| Real-time Chat (1-1 & Group) | ❌ | ✅ | ✅ | ✅ |
| Hệ thống Thông báo | ❌ | ✅ | ✅ | ✅ |
| Quản lý Profile & Sessions | ❌ | ✅ | ✅ | ✅ |
| Ghim / Khóa / Chuyển bài viết | ❌ | ❌ | ✅ | ✅ |
| Xóa bài/comment vi phạm | ❌ | ❌ | ✅ | ✅ |
| Xử lý Báo cáo (Reports) | ❌ | ❌ | ✅ | ✅ |
| Quản lý Categories & Tags | ❌ | ❌ | ❌ | ✅ |
| Quản lý & Phân quyền Người dùng | ❌ | ❌ | ❌ | ✅ |
| Khóa tài khoản (Ban User) | ❌ | ❌ | ❌ | ✅ |
| Dashboard & Metrics | ❌ | ❌ | ❌ | ✅ |
| Giám sát Rate Limiting / Spam | ❌ | ❌ | ❌ | ✅ |

---

*Tài liệu này là một phần của bộ Detailed Specification cho dự án **Forum-Flow**.*  
*Xem các chương tiếp theo để biết chi tiết về Kiến trúc Hệ thống, Thiết kế Cơ sở Dữ liệu và Đặc tả API.*
