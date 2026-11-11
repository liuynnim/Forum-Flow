namespace ForumFlow.Domain.Entities;

/// <summary>
/// Ánh xạ bảng `post_edit_history` — lưu lịch sử chỉnh sửa bài viết.
/// </summary>
public class PostEditHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PreviousTitle { get; set; } = string.Empty;
    public string PreviousContent { get; set; } = string.Empty;
    public string? EditReason { get; set; } // VARCHAR(255)
    public DateTime EditedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public Guid EditorId { get; set; }
    public User Editor { get; set; } = null!;
}
