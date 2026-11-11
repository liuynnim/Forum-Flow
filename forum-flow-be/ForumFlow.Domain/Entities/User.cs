using ForumFlow.Domain.Common;
using ForumFlow.Domain.Enums;

namespace ForumFlow.Domain.Entities;

/// <summary>
/// Ánh xạ bảng <c>users</c> — bảng trung tâm của hệ thống.
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;       // VARCHAR(50) UNIQUE, ≥3 ký tự
    public string Email { get; set; } = string.Empty;          // VARCHAR(255) UNIQUE
    public string? PasswordHash { get; set; }                  // NULL nếu chỉ dùng OAuth
    public string DisplayName { get; set; } = string.Empty;    // VARCHAR(100) NOT NULL
    public string? AvatarUrl { get; set; }                     // URL từ Cloudinary
    public string? Bio { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? GithubUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.Member;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsBanned { get; set; } = false;
    public string? BanReason { get; set; }
    public DateTime? BanUntil { get; set; }                    // NULL = ban vĩnh viễn
    public DateTime? LastSeenAt { get; set; }

    // Navigation Properties
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostVote> PostVotes { get; set; } = new List<PostVote>();
    public ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}
